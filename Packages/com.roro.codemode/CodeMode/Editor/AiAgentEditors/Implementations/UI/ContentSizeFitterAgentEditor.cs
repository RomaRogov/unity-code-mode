using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(ContentSizeFitter))]
    public class ContentSizeFitterAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_HorizontalFit;
        private SerializedProperty m_VerticalFit;

        protected override void OnEnable()
        {
            m_HorizontalFit = serializedObject.FindProperty("m_HorizontalFit");
            m_VerticalFit = serializedObject.FindProperty("m_VerticalFit");

            AddSettingPropertyHandler("horizontalFit",
                () => SerializeEnumToJValue(m_HorizontalFit),
                v => SetEnumValue(m_HorizontalFit, v));

            AddSettingPropertyHandler("verticalFit",
                () => SerializeEnumToJValue(m_VerticalFit),
                v => SetEnumValue(m_VerticalFit, v));
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("horizontalFit", SerializeEnumToJValue(m_HorizontalFit));
            DumpProperty("verticalFit", SerializeEnumToJValue(m_VerticalFit));
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(ContentSizeFitter.FitMode));

            EmitClassDefinition("ContentSizeFitter", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("horizontalFit", "FitMode"),
                TsPropertyDef.Field("verticalFit", "FitMode"),
            });
        }
    }
}

