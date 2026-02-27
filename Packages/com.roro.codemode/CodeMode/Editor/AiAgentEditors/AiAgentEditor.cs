using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.AiAgentEditors
{
    public partial class AiAgentEditor : AiEditorBase
    {
        public Object target { get; private set; }
        public SerializedObject serializedObject { get; private set; }

        private static Dictionary<Type, Type> _editorTypeCache;

        public static AiAgentEditor CreateFor(Object instance)
        {
            if (_editorTypeCache == null)
                BuildEditorTypeCache();

            var editor = FindEditorForType(instance.GetType());
            editor.target = instance;
            editor.serializedObject = new SerializedObject(editor.GetInspectionTarget());
            editor.OnEnable();
            return editor;
        }

        private static void BuildEditorTypeCache()
        {
            _editorTypeCache = new Dictionary<Type, Type>();
            var editorTypes = TypeCache.GetTypesWithAttribute<CustomAiAgentEditorAttribute>();
            foreach (var editorType in editorTypes)
            {
                var attr = (CustomAiAgentEditorAttribute)Attribute.GetCustomAttribute(
                    editorType, typeof(CustomAiAgentEditorAttribute));
                if (attr != null)
                    _editorTypeCache[attr.InspectedType] = editorType;
            }
        }

        private static AiAgentEditor FindEditorForType(Type instanceType)
        {
            var current = instanceType;
            while (current != null)
            {
                if (_editorTypeCache.TryGetValue(current, out var editorType))
                    return (AiAgentEditor)Activator.CreateInstance(editorType);
                current = current.BaseType;
            }

            return new AiAgentEditor();
        }

        protected Object GetInspectionTarget()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && importer.GetType() != typeof(AssetImporter))
                    return importer;
            }
            return target;
        }
        
        protected static T ParseEnum<T>(JToken value) where T : struct
        {
            if (value.Type == JTokenType.String)
            {
                if (Enum.TryParse<T>(value.Value<string>(), out var result))
                    return result;
            }

            return (T)(object)value.Value<int>();
        }
    }
}
