using System;
using CodeMode.Editor.CustomSettingsEditors;
using CodeMode.Editor.Tools.Attributes;
using Newtonsoft.Json.Linq;

namespace CodeMode.Editor.Tools.Implementations
{
    public static class SettingsTools
    {
        public enum SettingsType
        {
            Physics,
            Physics2D,
            Lighting
        }

        [UtcpTool("Gets properties for project settings by type (Physics, Physics2D, Lighting, etc.).",
            httpMethod: "GET",
            tags: new[] { "settings", "properties", "project", "physics", "physics2d", "lighting", "inspect" })]
        public static InspectorTools.DumpResult SettingsGetProperties(SettingsType settingsType)
        {
            var editor = AiSettingsEditor.CreateFor(settingsType.ToString());
            return new InspectorTools.DumpResult { dump = editor.BuildDump() };
        }

        [UtcpTool("Sets properties on project settings. Property paths and types must be confirmed via SettingsGet* tools first.",
            httpMethod: "POST",
            tags: new[] { "settings", "property", "set", "project", "physics", "physics2d", "lighting", "modify" })]
        public static void SettingsSetProperties(SettingsType settingsType, string[] propertyPaths, JToken[] values)
        {
            if (propertyPaths == null || propertyPaths.Length == 0)
                throw new Exception("propertyPaths is required.");

            if (propertyPaths.Length != values.Length)
                throw new Exception("propertyPaths and values arrays must have the same length.");

            for (int i = 0; i < propertyPaths.Length; i++)
            {
                if (string.IsNullOrEmpty(propertyPaths[i]))
                    throw new Exception($"propertyPath at index {i} is null or empty.");
            }

            try
            {
                var editor = AiSettingsEditor.CreateFor(settingsType.ToString());
                editor.SetProperties(propertyPaths, values);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set settings properties: {ex.Message}");
            }
        }

        [UtcpTool("Generates TypeScript definition for project settings or common types.",
            httpMethod: "GET",
            tags: new[] { "settings", "definition", "typescript", "code", "inspect" })]
        public static InspectorTools.DefinitionResult SettingsGetDefinition(SettingsType settingsType)
        {
            var editor = AiSettingsEditor.CreateFor(settingsType.ToString());
            return new InspectorTools.DefinitionResult { definition = editor.BuildDefinition() };
        }
    }
}
