using System;
using System.Collections.Generic;
using System.Reflection;
using CodeMode.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.AiAgentEditors
{
    public partial class AiAgentEditor
    {
        private static readonly HashSet<Type> UnityBaseTypes = new HashSet<Type>
        {
            typeof(Object),
            typeof(Component),
            typeof(Behaviour),
            typeof(MonoBehaviour),
            typeof(ScriptableObject),
        };

        protected override void OnDefinitionRequested()
        {
            var inspectTarget = serializedObject.targetObject;
            EmitClassFromData(inspectTarget.GetType().Name,
                SerializationUtils.ProcessSerializedObject(serializedObject));
        }

        #region Emit Helpers

        protected void EmitClassFromData(string className, SerializationUtils.SerializedObjectData data)
        {
            if (_definedNames.Contains(className)) return;
            _definedNames.Add(className);

            var fields = new List<TsPropertyDef>();

            foreach (var p in data.properties)
            {
                if (!fields.Exists(f => f.Name == p.name))
                    fields.Add(DefineProperty(p));
            }


            foreach (var s in data.structs)
            {
                var typeName = GetStructTypeName(s.property);
                EmitClassFromData(typeName, s.children);
                fields.Add(DefineProperty(s.property, typeName));
            }

            foreach (var a in data.arrays)
                fields.Add(DefineProperty(a.property, $"Array<{ResolveArrayElementType(a)}>"));

            EmitClassDefinition(className, fields);
        }

        #endregion

        #region Property Definition Builders

        protected TsPropertyDef DefineProperty(SerializedProperty property)
            => DefineProperty(property, MapPropertyType(property));

        protected TsPropertyDef DefineProperty(SerializedProperty property, string tsType)
        {
            var def = TsPropertyDef.Field(property.name, tsType);

            var fieldInfo = SerializationUtils.GetFieldInfoFromProperty(property, out _);
            if (fieldInfo != null)
            {
                var rawType = fieldInfo.FieldType;
                if (rawType.IsArray)
                    rawType = rawType.GetElementType();
                else if (rawType.IsGenericType && rawType.GetGenericTypeDefinition() == typeof(List<>))
                    rawType = rawType.GetGenericArguments()[0];
                ApplyFieldAttributes(def, fieldInfo, rawType);
            }

            return def;
        }

        protected TsPropertyDef DefineMember(MemberInfo member)
        {
            Type memberType;
            if (member is PropertyInfo prop) memberType = prop.PropertyType;
            else if (member is FieldInfo field) memberType = field.FieldType;
            else return null;

            string tsType;
            Type rawType = memberType;

            if (memberType.IsArray)
            {
                rawType = memberType.GetElementType();
                tsType = $"Array<{ResolveTsType(rawType)}>";
            }
            else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                rawType = memberType.GetGenericArguments()[0];
                tsType = $"Array<{ResolveTsType(rawType)}>";
            }
            else
            {
                tsType = ResolveTsType(memberType);
            }

            var def = TsPropertyDef.Field(member.Name, tsType);

            if (member is FieldInfo f)
                ApplyFieldAttributes(def, f, rawType);

            return def;
        }

        protected static void ApplyFieldAttributes(TsPropertyDef def, FieldInfo field, Type rawType)
        {
            var tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttr != null && !string.IsNullOrEmpty(tooltipAttr.tooltip))
                def.Comment = tooltipAttr.tooltip;

            var headerAttr = field.GetCustomAttribute<HeaderAttribute>();
            if (headerAttr != null)
                def.SectionHeader = headerAttr.header;

            var parts = new List<string>();

            if (rawType == typeof(int) || rawType == typeof(long) || rawType == typeof(short) || rawType == typeof(byte))
                parts.Add("type: Integer");
            else if (rawType == typeof(float) || rawType == typeof(double))
                parts.Add("type: Float");

            var rangeAttr = field.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                parts.Add($"min: {rangeAttr.min}");
                parts.Add($"max: {rangeAttr.max}");
            }
            else
            {
                var minAttr = field.GetCustomAttribute<MinAttribute>();
                if (minAttr != null)
                    parts.Add($"min: {minAttr.min}");
            }

            if (field.GetCustomAttribute<MultilineAttribute>() != null || field.GetCustomAttribute<TextAreaAttribute>() != null)
                parts.Add("multiline: true");

            if (parts.Count > 0)
                def.Decorator = string.Join(", ", parts);
        }

        #endregion

        #region Type Resolution

        private string MapPropertyType(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.FixedBufferSize:
                    return "number";
                case SerializedPropertyType.Boolean:
                    return "boolean";
                case SerializedPropertyType.String:
                case SerializedPropertyType.Character:
                    return "string";
                case SerializedPropertyType.Color: return "Color";
                case SerializedPropertyType.Vector2: return "Vector2";
                case SerializedPropertyType.Vector3: return "Vector3";
                case SerializedPropertyType.Vector4: return "Vector4";
                case SerializedPropertyType.Quaternion: return "Quaternion";
                case SerializedPropertyType.Rect: return "Rect";
                case SerializedPropertyType.Bounds: return "Bounds";
                case SerializedPropertyType.Vector2Int: return "Vector2Int";
                case SerializedPropertyType.Vector3Int: return "Vector3Int";
                case SerializedPropertyType.RectInt: return "RectInt";
                case SerializedPropertyType.BoundsInt: return "BoundsInt";
                case SerializedPropertyType.AnimationCurve: return "AnimationCurve";
                case SerializedPropertyType.Gradient: return "Gradient";

                case SerializedPropertyType.ObjectReference:
                    return $"InstanceReference<{property.type.Replace("PPtr<", "").Replace(">", "")}>";
                case SerializedPropertyType.ExposedReference:
                    return $"InstanceReference<{property.type}>";

                case SerializedPropertyType.Enum:
                    var fieldInfo = SerializationUtils.GetFieldInfoFromProperty(property, out _);
                    if (fieldInfo != null && fieldInfo.FieldType.IsEnum)
                    {
                        GenerateEnumDefinition(fieldInfo.FieldType);
                        return fieldInfo.FieldType.Name;
                    }
                    return "number";

                default:
                    return "any";
            }
        }

        protected static string GetStructTypeName(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var fullName = property.managedReferenceFullTypename;
                if (!string.IsNullOrEmpty(fullName))
                {
                    var spaceIdx = fullName.IndexOf(' ');
                    var qualifiedName = spaceIdx >= 0 ? fullName.Substring(spaceIdx + 1) : fullName;
                    var dotIdx = qualifiedName.LastIndexOf('.');
                    return dotIdx >= 0 ? qualifiedName.Substring(dotIdx + 1) : qualifiedName;
                }
            }
            return property.type;
        }

        protected string ResolveArrayElementType(SerializationUtils.ArrayEntry entry)
        {
            if (entry.elements != null)
            {
                var first = entry.property.GetArrayElementAtIndex(0);
                var typeName = GetStructTypeName(first);
                EmitClassFromData(typeName, entry.elements[0]);
                return typeName;
            }

            if (entry.property.arraySize > 0)
                return MapPropertyType(entry.property.GetArrayElementAtIndex(0));

            var fieldInfo = SerializationUtils.GetFieldInfoFromProperty(entry.property, out _);
            if (fieldInfo != null)
            {
                var elemType = GetCollectionElementType(fieldInfo.FieldType);
                if (elemType != null)
                    return ResolveTsType(elemType);
            }

            return "any";
        }

        protected string ResolveTsType(Type type)
        {
            if (type == null) return "any";

            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return ResolveTsType(underlying) + " | null";

            if (type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(long) ||
                type == typeof(short) || type == typeof(byte) || type == typeof(decimal)) return "number";
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(string) || type == typeof(char)) return "string";

            if (type == typeof(Vector2)) return "Vector2";
            if (type == typeof(Vector3)) return "Vector3";
            if (type == typeof(Vector4)) return "Vector4";
            if (type == typeof(Color) || type == typeof(Color32)) return "Color";
            if (type == typeof(Rect)) return "Rect";
            if (type == typeof(Bounds)) return "Bounds";
            if (type == typeof(Quaternion)) return "Quaternion";
            if (type == typeof(Vector2Int)) return "Vector2Int";
            if (type == typeof(Vector3Int)) return "Vector3Int";
            if (type == typeof(RectInt)) return "RectInt";
            if (type == typeof(BoundsInt)) return "BoundsInt";
            if (type == typeof(Matrix4x4)) return "Matrix4x4";
            if (type == typeof(AnimationCurve)) return "AnimationCurve";
            if (type == typeof(Gradient)) return "Gradient";
            if (type == typeof(LayerMask)) return "number";
            if (type == typeof(Scene)) return "string";

            if (type.IsEnum)
            {
                GenerateEnumDefinition(type);
                return type.Name;
            }

            if (type.IsArray)
                return $"Array<{ResolveTsType(type.GetElementType())}>";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return $"Array<{ResolveTsType(type.GetGenericArguments()[0])}>";

            if (typeof(Object).IsAssignableFrom(type))
                return $"Reference<{type.Name}>";

            if ((type.IsClass || (type.IsValueType && !type.IsPrimitive)) && type != typeof(object))
            {
                ProcessClass(type.Name, type);
                return type.Name;
            }

            return "any";
        }

        protected static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
                return collectionType.GetGenericArguments()[0];
            return null;
        }

        #endregion

        #region Class Processing (Reflection Fallback for Nested Types)

        protected void ProcessClass(string className, Type type)
        {
            if (_definedNames.Contains(className))
                return;

            _definedNames.Add(className);

            var fields = new List<TsPropertyDef>();
            var members = GetAccessibleMembers(type);

            foreach (var member in members)
            {
                var def = DefineMember(member);
                if (def != null)
                    fields.Add(def);
            }

            EmitClassDefinition(className, fields);
        }

        protected static List<MemberInfo> GetAccessibleMembers(Type type)
        {
            var members = new List<MemberInfo>();
            var seenNames = new HashSet<string>();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.DeclaringType != null && UnityBaseTypes.Contains(prop.DeclaringType)) continue;
                if (prop.GetCustomAttribute<ObsoleteAttribute>() != null) continue;
                if (prop.GetGetMethod() == null || prop.GetIndexParameters().Length > 0) continue;

                members.Add(prop);
                seenNames.Add(prop.Name);
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.DeclaringType != null && UnityBaseTypes.Contains(field.DeclaringType)) continue;
                if (field.GetCustomAttribute<ObsoleteAttribute>() != null) continue;
                if (!seenNames.Add(field.Name)) continue;

                members.Add(field);
            }

            var currentType = type;
            while (currentType != null && !UnityBaseTypes.Contains(currentType))
            {
                foreach (var field in currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.GetCustomAttribute<SerializeField>() != null &&
                        field.GetCustomAttribute<ObsoleteAttribute>() == null &&
                        !(currentType.Namespace != null && currentType.Namespace.StartsWith("UnityEngine.") && field.Name.StartsWith("m_")) &&
                        seenNames.Add(field.Name))
                    {
                        members.Add(field);
                    }
                }
                currentType = currentType.BaseType;
            }

            return members;
        }

        #endregion
    }
}
