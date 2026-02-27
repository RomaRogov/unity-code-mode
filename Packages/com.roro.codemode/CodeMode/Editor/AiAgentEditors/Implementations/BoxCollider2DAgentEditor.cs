using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(BoxCollider2D))]
    public class BoxCollider2DAgentEditor : Collider2DAgentEditor
    {
        private SerializedProperty m_Size;
        private SerializedProperty m_EdgeRadius;
        private SerializedProperty m_AutoTiling;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Size = serializedObject.FindProperty("m_Size");
            m_EdgeRadius = serializedObject.FindProperty("m_EdgeRadius");
            m_AutoTiling = serializedObject.FindProperty("m_AutoTiling");

            AddSettingPropertyHandler("size",
                () => m_Size.vector2Value.SerializeToJObject(),
                v => m_Size.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("edgeRadius",
                () => new JValue(m_EdgeRadius.floatValue),
                v => m_EdgeRadius.floatValue = v.Value<float>());

            AddSettingPropertyHandler("autoTiling",
                () => new JValue(m_AutoTiling.boolValue),
                v => m_AutoTiling.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("size", m_Size.vector2Value.SerializeToJObject());
            DumpProperty("edgeRadius", m_EdgeRadius.floatValue);
            DumpProperty("autoTiling", m_AutoTiling.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            EmitClassDefinition("BoxCollider2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("size", "Vector2"),
                TsPropertyDef.Field("edgeRadius", "number"),
                TsPropertyDef.Field("autoTiling", "boolean"),
            }, "Collider2D");
        }
    }
}
