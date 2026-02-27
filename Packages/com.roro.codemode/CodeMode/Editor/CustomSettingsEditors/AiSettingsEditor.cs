using System;
using System.Collections.Generic;
using CodeMode.Editor.AiAgentEditors;
using CodeMode.Editor.CustomSettingsEditors.Implementations;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace CodeMode.Editor.CustomSettingsEditors
{
    public class AiSettingsEditor : AiEditorBase
    {
        public string settingsName { get; private set; }

        private static Dictionary<string, Type> _editorTypeCache;

        public static AiSettingsEditor CreateFor(string settingsName)
        {
            if (_editorTypeCache == null)
                BuildEditorTypeCache();

            if (!_editorTypeCache.TryGetValue(settingsName, out var editorType))
                throw new Exception($"No settings editor registered for '{settingsName}'.");

            var editor = (AiSettingsEditor)Activator.CreateInstance(editorType);
            editor.settingsName = settingsName;
            editor.OnEnable();
            return editor;
        }

        private static void BuildEditorTypeCache()
        {
            _editorTypeCache = new Dictionary<string, Type>();
            var editorTypes = TypeCache.GetTypesWithAttribute<CustomSettingsEditorAttribute>();
            foreach (var editorType in editorTypes)
            {
                var attr = (CustomSettingsEditorAttribute)Attribute.GetCustomAttribute(
                    editorType, typeof(CustomSettingsEditorAttribute));
                if (attr != null)
                    _editorTypeCache[attr.SettingsName] = editorType;
            }
        }

        public void SetProperties(string[] propertyPaths, JToken[] values)
        {
            if (propertyPaths == null) throw new ArgumentNullException(nameof(propertyPaths));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (propertyPaths.Length != values.Length)
                throw new ArgumentException("Property paths and values arrays must have the same length.");

            for (int i = 0; i < propertyPaths.Length; i++)
                SetProperty(propertyPaths[i], values[i]);
        }

        public void SetProperty(string propertyPath, JToken value)
        {
            if (string.IsNullOrEmpty(propertyPath))
                throw new ArgumentNullException(nameof(propertyPath));

            var (handlerPath, remainingPath) = FindHandlerForPath(propertyPath);

            if (handlerPath != null)
            {
                var setHandler = _propertySetHandlers[handlerPath];

                if (_propertyGetHandlers != null &&
                    _propertyGetHandlers.TryGetValue(handlerPath, out var getHandler))
                {
                    var current = getHandler();
                    var merged = ApplyPatch(current, remainingPath, value);
                    setHandler(merged);
                }
                else
                {
                    if (remainingPath != null)
                        throw new Exception(
                            $"Cannot set subpath '{remainingPath}' on '{handlerPath}' without a getHandler.");
                    setHandler(value);
                }

                return;
            }

            throw new Exception($"Property '{propertyPath}' not recognized for {settingsName} settings.");
        }
    }
}
