using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace CodeMode.Editor.Utilities
{
    public static class SerializationUtils
    {
        public struct StructEntry
        {
            public SerializedProperty property;
            public SerializedObjectData children;
        }

        public struct ArrayEntry
        {
            public SerializedProperty property;
            public SerializedObjectData[] elements; // null for value-type arrays
        }

        public class SerializedObjectData
        {
            public List<SerializedProperty> properties = new();
            public List<StructEntry> structs = new();
            public List<ArrayEntry> arrays = new();
        }

        public static SerializedObjectData ProcessSerializedObject(SerializedObject serializedObject)
        {
            var result = new SerializedObjectData();
            var iterator = serializedObject.GetIterator();

            if (!iterator.NextVisible(true)) return result;

            do
            {
                if (iterator.name == "m_Script") continue;
                ProcessEntry(iterator, result);
            }
            while (iterator.NextVisible(false));

            return result;
        }

        public static SerializedObjectData ProcessSerializedProperty(SerializedProperty property)
        {
            var result = new SerializedObjectData();
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            if (!iterator.NextVisible(true)) return result;

            do
            {
                if (SerializedProperty.EqualContents(iterator, end)) break;
                ProcessEntry(iterator, result);
            }
            while (iterator.NextVisible(false));

            return result;
        }

        private static void ProcessEntry(SerializedProperty property, SerializedObjectData data)
        {
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                SerializedObjectData[] elements = null;
                if (property.arraySize > 0 && IsStructured(property.GetArrayElementAtIndex(0)))
                {
                    elements = new SerializedObjectData[property.arraySize];
                    for (int i = 0; i < property.arraySize; i++)
                        elements[i] = ProcessSerializedProperty(property.GetArrayElementAtIndex(i));
                }

                data.arrays.Add(new ArrayEntry
                {
                    property = property.Copy(),
                    elements = elements
                });
            }
            else if (IsStructured(property))
            {
                data.structs.Add(new StructEntry
                {
                    property = property.Copy(),
                    children = ProcessSerializedProperty(property)
                });
            }
            else
            {
                data.properties.Add(property.Copy());
            }
        }

        private static bool IsStructured(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic
                || property.propertyType == SerializedPropertyType.ManagedReference;
        }

        public static FieldInfo GetFieldInfoFromProperty(SerializedProperty property, out Type hostType)
        {
            var targetObject = property.serializedObject.targetObject;
            hostType = targetObject.GetType();
            FieldInfo field = null;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var parts = path.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (part.EndsWith("]"))
                {
                    var arrayName = part.Substring(0, part.IndexOf('['));
                    field = GetFieldInType(hostType, arrayName);
                    if (field == null) return null;

                    var fieldType = field.FieldType;
                    if (fieldType.IsArray)
                        hostType = fieldType.GetElementType();
                    else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                        hostType = fieldType.GetGenericArguments()[0];
                    else
                        return null;
                }
                else
                {
                    field = GetFieldInType(hostType, part);
                    if (field == null) return null;
                    hostType = field.FieldType;
                }
            }

            return field;
        }

        private static FieldInfo GetFieldInType(Type type, string name)
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                var field = current.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) return field;
                current = current.BaseType;
            }
            return null;
        }
    }
}