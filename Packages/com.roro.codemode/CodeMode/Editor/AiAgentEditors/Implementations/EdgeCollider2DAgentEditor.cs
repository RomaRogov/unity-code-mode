using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(EdgeCollider2D))]
    public class EdgeCollider2DAgentEditor : Collider2DAgentEditor
    {
        private SerializedProperty m_EdgeRadius;
        private SerializedProperty m_UseAdjacentStartPoint;
        private SerializedProperty m_UseAdjacentEndPoint;
        private SerializedProperty m_AdjacentStartPoint;
        private SerializedProperty m_AdjacentEndPoint;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_EdgeRadius = serializedObject.FindProperty("m_EdgeRadius");
            m_UseAdjacentStartPoint = serializedObject.FindProperty("m_UseAdjacentStartPoint");
            m_UseAdjacentEndPoint = serializedObject.FindProperty("m_UseAdjacentEndPoint");
            m_AdjacentStartPoint = serializedObject.FindProperty("m_AdjacentStartPoint");
            m_AdjacentEndPoint = serializedObject.FindProperty("m_AdjacentEndPoint");

            AddSettingPropertyHandler("edgeRadius",
                () => new JValue(m_EdgeRadius.floatValue),
                v => m_EdgeRadius.floatValue = v.Value<float>());

            AddSettingPropertyHandler("useAdjacentStartPoint",
                () => new JValue(m_UseAdjacentStartPoint.boolValue),
                v => m_UseAdjacentStartPoint.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("useAdjacentEndPoint",
                () => new JValue(m_UseAdjacentEndPoint.boolValue),
                v => m_UseAdjacentEndPoint.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("adjacentStartPoint",
                () => m_AdjacentStartPoint.vector2Value.SerializeToJObject(),
                v => m_AdjacentStartPoint.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("adjacentEndPoint",
                () => m_AdjacentEndPoint.vector2Value.SerializeToJObject(),
                v => m_AdjacentEndPoint.vector2Value = v.DeserializeToVector2());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("edgeRadius", m_EdgeRadius.floatValue);
            DumpProperty("useAdjacentStartPoint", m_UseAdjacentStartPoint.boolValue);
            DumpProperty("useAdjacentEndPoint", m_UseAdjacentEndPoint.boolValue);
            DumpProperty("adjacentStartPoint", m_AdjacentStartPoint.vector2Value.SerializeToJObject());
            DumpProperty("adjacentEndPoint", m_AdjacentEndPoint.vector2Value.SerializeToJObject());

            var edge = (EdgeCollider2D)target;
            DumpProperty("pointCount", edge.pointCount);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            EmitClassDefinition("EdgeCollider2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("edgeRadius", "number"),
                TsPropertyDef.Field("useAdjacentStartPoint", "boolean"),
                TsPropertyDef.Field("useAdjacentEndPoint", "boolean"),
                TsPropertyDef.Field("adjacentStartPoint", "Vector2"),
                TsPropertyDef.Field("adjacentEndPoint", "Vector2"),
                TsPropertyDef.Field("pointCount", "number").Readonly(),
            }, "Collider2D");
        }
    }
}
