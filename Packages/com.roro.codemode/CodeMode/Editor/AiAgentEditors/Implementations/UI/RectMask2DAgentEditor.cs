using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(RectMask2D))]
    public class RectMask2DAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Padding;
        private SerializedProperty m_Softness;

        protected override void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Softness = serializedObject.FindProperty("m_Softness");

            AddSettingPropertyHandler("padding",
                () => m_Padding.vector4Value.SerializeToJObject(),
                v => m_Padding.vector4Value = v.DeserializeToVector4());

            AddSettingPropertyHandler("softness",
                () => m_Softness.vector2IntValue.SerializeToJObject(),
                v => m_Softness.vector2IntValue = v.DeserializeToVector2Int());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("padding", m_Padding.vector4Value.SerializeToJObject());
            DumpProperty("softness", m_Softness.vector2IntValue.SerializeToJObject());
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("RectMask2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("padding", "Vector4"),
                TsPropertyDef.Field("softness", "Vector2Int"),
            });
        }
    }
}

