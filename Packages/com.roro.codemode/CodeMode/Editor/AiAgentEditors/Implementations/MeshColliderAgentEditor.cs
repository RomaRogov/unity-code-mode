using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(MeshCollider))]
    public class MeshColliderAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_Convex;
        private SerializedProperty m_CookingOptions;
        private SerializedProperty m_Mesh;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Convex = serializedObject.FindProperty("m_Convex");
            m_CookingOptions = serializedObject.FindProperty("m_CookingOptions");
            m_Mesh = serializedObject.FindProperty("m_Mesh");

            AddSettingPropertyHandler("convex",
                () => new JValue(m_Convex.boolValue),
                v => m_Convex.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("cookingOptions",
                () => new JValue(m_CookingOptions.intValue),
                v => m_CookingOptions.intValue = v.Value<int>());

            AddSettingPropertyHandler("sharedMesh",
                () => SerializeInstanceReferenceToJToken(m_Mesh.objectReferenceValue),
                v => SerializeInstanceReferenceToJToken(m_Mesh.objectReferenceValue));
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("convex", m_Convex.boolValue);
            DumpProperty("cookingOptions", m_CookingOptions.intValue);
            DumpProperty("sharedMesh", SerializeInstanceReferenceToJToken(m_Mesh.objectReferenceValue));
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            GenerateEnumDefinition(typeof(MeshColliderCookingOptions));

            EmitClassDefinition("MeshCollider", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("convex", "boolean"),
                TsPropertyDef.Field("cookingOptions", "number")
                    .WithComment("Bitmask of MeshColliderCookingOptions flags"),
                TsPropertyDef.Reference("sharedMesh", "Mesh").Nullable(),
            }, "Collider");
        }
    }
}
