using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(PolygonCollider2D))]
    public class PolygonCollider2DAgentEditor : Collider2DAgentEditor
    {
        private SerializedProperty m_AutoTiling;
        private SerializedProperty m_UseDelaunayMesh;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_AutoTiling = serializedObject.FindProperty("m_AutoTiling");
            m_UseDelaunayMesh = serializedObject.FindProperty("m_UseDelaunayMesh");

            if (m_AutoTiling != null)
            {
                AddSettingPropertyHandler("autoTiling",
                    () => new JValue(m_AutoTiling.boolValue),
                    v => m_AutoTiling.boolValue = v.Value<bool>());
            }

            if (m_UseDelaunayMesh != null)
            {
                AddSettingPropertyHandler("useDelaunayMesh",
                    () => new JValue(m_UseDelaunayMesh.boolValue),
                    v => m_UseDelaunayMesh.boolValue = v.Value<bool>());
            }

            // pathCount exposed as read-only via dump
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            if (m_AutoTiling != null)
                DumpProperty("autoTiling", m_AutoTiling.boolValue);
            if (m_UseDelaunayMesh != null)
                DumpProperty("useDelaunayMesh", m_UseDelaunayMesh.boolValue);

            var poly = (PolygonCollider2D)target;
            DumpProperty("pathCount", poly.pathCount);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            var fields = new List<TsPropertyDef>();
            if (m_AutoTiling != null)
                fields.Add(TsPropertyDef.Field("autoTiling", "boolean"));
            if (m_UseDelaunayMesh != null)
                fields.Add(TsPropertyDef.Field("useDelaunayMesh", "boolean"));
            fields.Add(TsPropertyDef.Field("pathCount", "number").Readonly());

            EmitClassDefinition("PolygonCollider2D", fields, "Collider2D");
        }
    }
}
