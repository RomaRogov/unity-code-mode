using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(CapsuleCollider2D))]
    public class CapsuleCollider2DAgentEditor : Collider2DAgentEditor
    {
        private SerializedProperty m_Size;
        private SerializedProperty m_Direction;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Size = serializedObject.FindProperty("m_Size");
            m_Direction = serializedObject.FindProperty("m_Direction");

            AddSettingPropertyHandler("size",
                () => m_Size.vector2Value.SerializeToJObject(),
                v => m_Size.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("direction",
                () => SerializePropertyValue(m_Direction),
                v => SetEnumValue(m_Direction, v));
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("size", m_Size.vector2Value.SerializeToJObject());
            DumpProperty("direction", SerializePropertyValue(m_Direction));
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            GenerateEnumDefinition(typeof(CapsuleDirection2D));
            EmitClassDefinition("CapsuleCollider2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("size", "Vector2"),
                TsPropertyDef.Field("direction", "CapsuleDirection2D"),
            }, "Collider2D");
        }
    }
}
