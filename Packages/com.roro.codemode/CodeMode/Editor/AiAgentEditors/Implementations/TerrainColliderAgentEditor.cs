using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(TerrainCollider))]
    public class TerrainColliderAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_TerrainData;
        private SerializedProperty m_EnableTreeColliders;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_TerrainData = serializedObject.FindProperty("m_TerrainData");
            m_EnableTreeColliders = serializedObject.FindProperty("m_EnableTreeColliders");

            if (m_TerrainData != null)
            {
                AddSettingPropertyHandler("terrainData",
                    () => SerializeInstanceReferenceToJToken(m_TerrainData.objectReferenceValue),
                    v => SetObjectReferenceWithJTokenInstance(m_TerrainData, v));
            }

            if (m_EnableTreeColliders != null)
            {
                AddSettingPropertyHandler("enableTreeColliders",
                    () => new JValue(m_EnableTreeColliders.boolValue),
                    v => m_EnableTreeColliders.boolValue = v.ToObject<bool>());
            }
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            if (m_TerrainData != null)
                DumpProperty("terrainData", SerializeInstanceReferenceToJToken(m_TerrainData.objectReferenceValue));
            if (m_EnableTreeColliders != null)
                DumpProperty("enableTreeColliders", m_EnableTreeColliders.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            var fields = new List<TsPropertyDef>();
            if (m_TerrainData != null)
                fields.Add(TsPropertyDef.Reference("terrainData", "TerrainData").Nullable());
            if (m_EnableTreeColliders != null)
                fields.Add(TsPropertyDef.Field("enableTreeColliders", "boolean"));

            EmitClassDefinition("TerrainCollider", fields, "Collider");
        }
    }
}
