using System;
using CodeMode.Editor.AiAgentEditors;
using CodeMode.Editor.Tools.Attributes;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations
{
    public class InspectorTools
    {
        #region GetDefinition
        
        public class DefinitionResult
        {
            public string definition;
        }

        [UtcpTool("Generates TypeScript definition based on properties and descriptions of an instance (Component, GameObject, Asset).",
            httpMethod: "GET",
            tags: new[] { "code", "typescript", "inspection", "definition", "class", "info", "meta", "instance", "node", "component", "asset", "data" })]
        public static DefinitionResult InspectorGetInstanceDefinition(InstanceReference<Object> reference)
        {
            var instance = reference?.Instance;
            if (instance == null)
                throw new Exception($"Instance not found for reference: '{reference?.id}'.");

            var editor = AiAgentEditor.CreateFor(instance);
            return new DefinitionResult { definition = editor.BuildDefinition() };
        }

        #endregion
        
        #region GetProperties

        [Serializable]
        public class DumpResult
        {
            public object dump;
        }

        [UtcpTool(
            "Gets plain object of properties, with no serialization info for any instance (scene node, component, asset).",
            httpMethod: "GET",
            tags: new[] { "inspect", "properties", "dump", "instance", "node", "component", "asset", "data" })]
        public static DumpResult InspectorGetInstanceProperties(InstanceReference<Object> reference)
        {
            var instance = reference?.Instance;
            if (instance == null)
                throw new Exception($"Instance not found for reference: '{reference?.id}'.");

            var editor = AiAgentEditor.CreateFor(instance);
            return new DumpResult { dump = editor.BuildDump() };
        }
        #endregion

        #region SetProperty

        [UtcpTool("Sets properties on instance of Component, GameObject or Asset. Property path and type must be confirmed via InspectorGet* tools first. Arrays are handled with dot notation for indices (e.g. 'sharedMaterials.0')",
            httpMethod: "POST", bodyField: "value",
            tags: new[] { "property", "set", "instance", "node", "component", "asset", "modify" })]
        public static void InspectorSetInstanceProperties(
            InstanceReference<Object> reference,
            string[] propertyPaths,
            JToken[] values)
        {
            var instance = reference?.Instance;
            if (instance == null)
                throw new Exception($"Instance not found for reference: '{reference?.id}'.");

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
                var editor = AiAgentEditor.CreateFor(instance);
                editor.SetProperties(propertyPaths, values);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set properties at {reference.id}: {ex.Message}");
            }
        }

        #endregion
    }
}