using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeMode.Editor.Protocol;
using CodeMode.Editor.Tools.Attributes;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Tool = CodeMode.Editor.Protocol.Tool;

namespace CodeMode.Editor.Tools.Registry
{
    /// <summary>
    /// Registry for collecting and defining UTCP tools
    /// 
    /// </summary>
    public class ToolRegistry
    {
        private readonly Dictionary<string, ToolMetadata> _tools = new();

        public IReadOnlyDictionary<string, ToolMetadata> Tools => _tools;

        #region Getting Tools from Unity's TypeCache

        public void BuildRegistry()
        {
            _tools.Clear();
            
            var toolMethods = TypeCache.GetMethodsWithAttribute<UtcpToolAttribute>();
            
            foreach (var method in toolMethods)
            {
                if (!method.IsStatic)
                {
                    Debug.LogWarning($"[CodeMode] Tool method '{method.DeclaringType?.FullName}.{method.Name}' is not static and will be ignored.");
                    continue;
                }
                
                if (!method.IsPublic)
                {
                    Debug.LogWarning($"[CodeMode] Tool method '{method.DeclaringType?.FullName}.{method.Name}' is not public and will be ignored.");
                    continue;
                }
                
                var attr = method.GetCustomAttribute<UtcpToolAttribute>();
                if (attr == null) continue;

                var metadata = new ToolMetadata
                {
                    Name = method.Name,
                    Description = attr.description,
                    HttpMethod = attr.httpMethod,
                    Tags = attr.tags,
                    Method = method,
                    BodyFieldName = attr.bodyField
                };

                // Determine input mode: class-based or parameter-based
                var parameters = method.GetParameters();

                if (parameters.Length == 1 && typeof(UtcpInput).IsAssignableFrom(parameters[0].ParameterType))
                {
                    // Class-based input: single parameter extending UtcpInput
                    metadata.UsesInputClass = true;
                    metadata.InputType = parameters[0].ParameterType;
                    metadata.InputSchema = TypeToSchema(metadata.InputType);
                }
                else if (parameters.Length > 0)
                {
                    // Parameter-based input: direct method parameters
                    metadata.UsesInputClass = false;
                    metadata.Parameters = parameters;
                    metadata.InputSchema = BuildParameterSchema(parameters);
                }
                else
                {
                    // No parameters
                    metadata.UsesInputClass = false;
                    metadata.Parameters = Array.Empty<ParameterInfo>();
                    metadata.InputSchema = JsonSchema.Object();
                }

                // Output type from return type
                var returnType = method.ReturnType;
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(UniTask<>))
                {
                    metadata.OutputType = returnType.GetGenericArguments()[0];
                    // Pre-build awaiter delegate once to avoid per-call reflection
                    metadata.UniTaskAwaiter = (Func<object, UniTask<object>>)
                        typeof(ToolRegistry)
                            .GetMethod(nameof(BuildAwaiter), BindingFlags.Static | BindingFlags.NonPublic)!
                            .MakeGenericMethod(metadata.OutputType)
                            .Invoke(null, null);
                }
                else if (returnType != typeof(void) && returnType != typeof(UniTask))
                {
                    metadata.OutputType = returnType;
                }

                // Fire-and-forget: void or non-generic UniTask
                metadata.IsFireAndForget = returnType == typeof(void) || returnType == typeof(UniTask);

                if (metadata.IsFireAndForget)
                {
                    // Fire-and-forget tools always return { success: true }
                    var ffSchema = JsonSchema.Object();
                    ffSchema.Prop("success", JsonSchema.Boolean(), true);
                    metadata.OutputSchema = ffSchema;
                }
                else
                {
                    metadata.OutputSchema = metadata.OutputType != null ? TypeToSchema(metadata.OutputType) : JsonSchema.Object();
                }

                _tools[metadata.Name] = metadata;
            }
        }

        /// <summary>
        /// Build JSON schema from method parameters
        /// </summary>
        private JsonSchema BuildParameterSchema(ParameterInfo[] parameters)
        {
            var schema = JsonSchema.Object();

            foreach (var param in parameters)
            {
                var paramSchema = TypeToSchema(param.ParameterType);

                // Get description from DescriptionAttribute or Tooltip
                var desc = param.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description
                        ?? param.GetCustomAttribute<TooltipAttribute>()?.tooltip;
                if (desc != null) paramSchema.description = desc;

                // Required if no default value and not nullable
                var isRequired = !param.HasDefaultValue &&
                                 param.GetCustomAttribute<CanBeNullAttribute>() == null &&
                                 Nullable.GetUnderlyingType(param.ParameterType) == null;

                schema.Prop(param.Name, paramSchema, isRequired);
            }

            return schema;
        }

        #endregion

        #region Schema Generation

        private JsonSchema TypeToSchema(Type type, HashSet<Type> visited = null)
        {
            visited ??= new HashSet<Type>();

            // Nullable<T>
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return TypeToSchema(underlying, visited).WithNullable();

            // Primitives
            if (type == typeof(string)) return JsonSchema.String();
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return JsonSchema.Integer();
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return JsonSchema.Number();
            if (type == typeof(bool)) return JsonSchema.Boolean();

            // Enum
            if (type.IsEnum)
                return JsonSchema.Enum(null, Enum.GetNames(type));

            // Array/List
            if (type.IsArray)
                return JsonSchema.Array(TypeToSchema(type.GetElementType(), visited));
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return JsonSchema.Array(TypeToSchema(type.GetGenericArguments()[0], visited));

            // Object
            if (type.IsClass && type != typeof(string) && type != typeof(object) && type != typeof(JObject))
            {
                if (!visited.Add(type)) return JsonSchema.Object();
                var schema = BuildObjectSchema(type, visited);
                visited.Remove(type);
                return schema;
            }

            return JsonSchema.Object();
        }

        private JsonSchema BuildObjectSchema(Type type, HashSet<Type> visited)
        {
            var schema = JsonSchema.Object();

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var desc = field.GetCustomAttribute<TooltipAttribute>()?.tooltip;
                var fieldSchema = TypeToSchema(field.FieldType, visited);
                if (desc != null) fieldSchema.description = desc;

                var isRequired = field.GetCustomAttribute<CanBeNullAttribute>() == null 
                                 && Nullable.GetUnderlyingType(field.FieldType) == null;
                schema.Prop(field.Name, fieldSchema, isRequired);
            }

            return schema;
        }

        private static Func<object, UniTask<object>> BuildAwaiter<T>()
        {
            return async obj => await (UniTask<T>)obj;
        }

        #endregion

        #region UTCP Manual

        public UtcpManual GetUtcpManual(string baseUrl = "", string manualVersion = "1.0.0")
        {
            var manual = UtcpManual.Create(manualVersion);
            foreach (var tool in _tools.Values.OrderBy(t => t.Name))
            {
                manual.tools.Add(new Tool
                {
                    name = tool.Name,
                    description = tool.Description,
                    tags = tool.Tags?.ToList() ?? new List<string>(),
                    inputs = tool.InputSchema,
                    outputs = tool.OutputSchema,
                    tool_call_template = new HttpCallTemplate
                    {
                        http_method = tool.HttpMethod,
                        url = $"{baseUrl}/tools/{tool.Name}",
                        body_field = tool.BodyFieldName
                    }
                });
            }
            return manual;
        }

        #endregion
    }
}
