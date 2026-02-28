using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CodeMode.Editor.Protocol;
using CodeMode.Editor.Tools.Attributes;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Tool = CodeMode.Editor.Protocol.Tool;

namespace CodeMode.Editor.Tools.Registry
{
    /// <summary>
    /// Registry for collecting and defining UTCP tools
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
                    metadata.UsesInputClass = true;
                    metadata.InputType = parameters[0].ParameterType;
                    metadata.InputSchema = TypeToSchema(metadata.InputType);
                }
                else if (parameters.Length > 0)
                {
                    metadata.UsesInputClass = false;
                    metadata.Parameters = parameters;
                    metadata.InputSchema = BuildParameterSchema(parameters);
                }
                else
                {
                    metadata.UsesInputClass = false;
                    metadata.Parameters = Array.Empty<ParameterInfo>();
                    metadata.InputSchema = JsonSchema.Object();
                }

                // Classify return type and build async delegates
                var returnType = method.ReturnType;
                ClassifyReturnType(returnType, metadata);

                // Output schema
                if (metadata.IsFireAndForget)
                {
                    var ffSchema = JsonSchema.Object();
                    ffSchema.Prop("callDelayed", JsonSchema.Boolean(), true);
                    metadata.OutputSchema = ffSchema;
                }
                else
                {
                    metadata.OutputSchema = metadata.OutputType != null
                        ? TypeToSchema(metadata.OutputType)
                        : JsonSchema.Object();
                }

                _tools[metadata.Name] = metadata;
            }
        }

        private static void ClassifyReturnType(Type returnType, ToolMetadata metadata)
        {
            // generic Task — async with result
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                metadata.OutputType = returnType.GetGenericArguments()[0];
                metadata.IsFireAndForget = false;
                metadata.AsyncAwaiter = async obj => (object)await (dynamic)obj;
                return;
            }

            // generic UniTask — async with result (reflective, no compile-time dependency)
            if (IsUniTaskGeneric(returnType))
            {
                metadata.OutputType = returnType.GetGenericArguments()[0];
                metadata.IsFireAndForget = false;
                metadata.AsyncAwaiter = BuildReflectiveUniTaskAwaiter();
                return;
            }

            // Task — fire-and-forget async
            if (returnType == typeof(Task))
            {
                metadata.IsFireAndForget = true;
                metadata.FireAndForgetObserver = obj => (Task)obj;
                return;
            }

            // UniTask (non-generic) — fire-and-forget async (reflective)
            if (IsUniTaskNonGeneric(returnType))
            {
                metadata.IsFireAndForget = true;
                metadata.FireAndForgetObserver = BuildReflectiveUniTaskObserver();
                return;
            }

            // void — synchronous fire-and-forget
            if (returnType == typeof(void))
            {
                metadata.IsFireAndForget = true;
                // No observer needed for synchronous void
                return;
            }

            // Synchronous with result
            metadata.OutputType = returnType;
            metadata.IsFireAndForget = false;
        }

        private static bool IsUniTaskGeneric(Type type) =>
            type.IsGenericType &&
            type.GetGenericTypeDefinition().FullName == "Cysharp.Threading.Tasks.UniTask`1";

        private static bool IsUniTaskNonGeneric(Type type) =>
            type.FullName == "Cysharp.Threading.Tasks.UniTask";

        /// <summary>
        /// Builds an awaiter for generic UniTask using reflection (no compile-time UniTask dependency).
        /// Calls UniTask.AsTask() at runtime if available.
        /// </summary>
        private static Func<object, Task<object>> BuildReflectiveUniTaskAwaiter()
        {
            return async obj =>
            {
                if (obj == null) return null;
                var asTaskMethod = obj.GetType().GetMethod("AsTask");
                if (asTaskMethod == null)
                {
                    Debug.LogWarning("[CodeMode] UniTask.AsTask() not found — UniTask package may not be installed.");
                    return null;
                }
                var task = (Task)asTaskMethod.Invoke(obj, null); // generic Task
                await task.ConfigureAwait(false);
                return task.GetType().GetProperty("Result")?.GetValue(task);
            };
        }

        /// <summary>
        /// Builds an observer for non-generic UniTask using reflection.
        /// Calls UniTask.AsTask() at runtime if available.
        /// </summary>
        private static Func<object, Task> BuildReflectiveUniTaskObserver()
        {
            return obj =>
            {
                if (obj == null) return Task.CompletedTask;
                var asTaskMethod = obj.GetType().GetMethod("AsTask");
                if (asTaskMethod == null) return Task.CompletedTask;
                return (Task)asTaskMethod.Invoke(obj, null);
            };
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

                var desc = param.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description
                        ?? param.GetCustomAttribute<TooltipAttribute>()?.tooltip;
                if (desc != null) paramSchema.description = desc;

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

            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return TypeToSchema(underlying, visited).WithNullable();

            if (type == typeof(string)) return JsonSchema.String();
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return JsonSchema.Integer();
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return JsonSchema.Number();
            if (type == typeof(bool)) return JsonSchema.Boolean();

            if (type.IsEnum)
                return JsonSchema.Enum(null, Enum.GetNames(type));

            if (type.IsArray)
                return JsonSchema.Array(TypeToSchema(type.GetElementType(), visited));
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return JsonSchema.Array(TypeToSchema(type.GetGenericArguments()[0], visited));

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
