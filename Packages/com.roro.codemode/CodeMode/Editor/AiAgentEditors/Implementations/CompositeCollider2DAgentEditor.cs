using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(CompositeCollider2D))]
    public class CompositeCollider2DAgentEditor : Collider2DAgentEditor
    {
        private SerializedProperty m_GeometryType;
        private SerializedProperty m_GenerationType;
        private SerializedProperty m_EdgeRadius;
        private SerializedProperty m_VertexDistance;
        private SerializedProperty m_OffsetDistance;
        private SerializedProperty m_UseDelaunayMesh;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_GeometryType = serializedObject.FindProperty("m_GeometryType");
            m_GenerationType = serializedObject.FindProperty("m_GenerationType");
            m_EdgeRadius = serializedObject.FindProperty("m_EdgeRadius");
            m_VertexDistance = serializedObject.FindProperty("m_VertexDistance");
            m_OffsetDistance = serializedObject.FindProperty("m_OffsetDistance");
            m_UseDelaunayMesh = serializedObject.FindProperty("m_UseDelaunayMesh");

            AddSettingPropertyHandler("geometryType",
                () => SerializeEnumToJValue(m_GeometryType),
                v => SetEnumValue(m_GeometryType, v));

            AddSettingPropertyHandler("generationType",
                () => SerializeEnumToJValue(m_GenerationType),
                v => SetEnumValue(m_GenerationType, v));

            AddSettingPropertyHandler("edgeRadius",
                () => new JValue(m_EdgeRadius.floatValue),
                v => m_EdgeRadius.floatValue = v.Value<float>());

            AddSettingPropertyHandler("vertexDistance",
                () => new JValue(m_VertexDistance.floatValue),
                v => m_VertexDistance.floatValue = v.Value<float>());

            if (m_OffsetDistance != null)
            {
                AddSettingPropertyHandler("offsetDistance",
                    () => new JValue(m_OffsetDistance.floatValue),
                    v => m_OffsetDistance.floatValue = v.Value<float>());
            }

            if (m_UseDelaunayMesh != null)
            {
                AddSettingPropertyHandler("useDelaunayMesh",
                    () => new JValue(m_UseDelaunayMesh.boolValue),
                    v => m_UseDelaunayMesh.boolValue = v.Value<bool>());
            }
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("geometryType", SerializeEnumToJValue(m_GeometryType));
            DumpProperty("generationType", SerializeEnumToJValue(m_GenerationType));
            DumpProperty("edgeRadius", m_EdgeRadius.floatValue);
            DumpProperty("vertexDistance", m_VertexDistance.floatValue);
            if (m_OffsetDistance != null)
                DumpProperty("offsetDistance", m_OffsetDistance.floatValue);
            if (m_UseDelaunayMesh != null)
                DumpProperty("useDelaunayMesh", m_UseDelaunayMesh.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            GenerateEnumDefinition(typeof(CompositeCollider2D.GeometryType));
            GenerateEnumDefinition(typeof(CompositeCollider2D.GenerationType));

            var fields = new List<TsPropertyDef>
            {
                TsPropertyDef.Field("geometryType", "GeometryType"),
                TsPropertyDef.Field("generationType", "GenerationType"),
                TsPropertyDef.Field("edgeRadius", "number"),
                TsPropertyDef.Field("vertexDistance", "number"),
            };

            if (m_OffsetDistance != null)
                fields.Add(TsPropertyDef.Field("offsetDistance", "number"));
            if (m_UseDelaunayMesh != null)
                fields.Add(TsPropertyDef.Field("useDelaunayMesh", "boolean"));

            EmitClassDefinition("CompositeCollider2D", fields, "Collider2D");
        }
    }
}
