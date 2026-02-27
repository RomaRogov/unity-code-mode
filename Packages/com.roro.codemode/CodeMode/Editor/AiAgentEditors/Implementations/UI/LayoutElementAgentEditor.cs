using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(LayoutElement))]
    public class LayoutElementAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_IgnoreLayout;
        private SerializedProperty m_MinWidth;
        private SerializedProperty m_MinHeight;
        private SerializedProperty m_PreferredWidth;
        private SerializedProperty m_PreferredHeight;
        private SerializedProperty m_FlexibleWidth;
        private SerializedProperty m_FlexibleHeight;
        private SerializedProperty m_LayoutPriority;

        protected override void OnEnable()
        {
            m_IgnoreLayout = serializedObject.FindProperty("m_IgnoreLayout");
            m_MinWidth = serializedObject.FindProperty("m_MinWidth");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
            m_PreferredWidth = serializedObject.FindProperty("m_PreferredWidth");
            m_PreferredHeight = serializedObject.FindProperty("m_PreferredHeight");
            m_FlexibleWidth = serializedObject.FindProperty("m_FlexibleWidth");
            m_FlexibleHeight = serializedObject.FindProperty("m_FlexibleHeight");
            m_LayoutPriority = serializedObject.FindProperty("m_LayoutPriority");

            AddSettingPropertyHandler("ignoreLayout",
                () => new JValue(m_IgnoreLayout.boolValue),
                v => m_IgnoreLayout.boolValue = v.Value<bool>());

            // Float properties use -1 as "not set" (disabled in Inspector)
            AddSettingPropertyHandler("minWidth",
                () => new JValue(m_MinWidth.floatValue),
                v => m_MinWidth.floatValue = v.Value<float>());

            AddSettingPropertyHandler("minHeight",
                () => new JValue(m_MinHeight.floatValue),
                v => m_MinHeight.floatValue = v.Value<float>());

            AddSettingPropertyHandler("preferredWidth",
                () => new JValue(m_PreferredWidth.floatValue),
                v => m_PreferredWidth.floatValue = v.Value<float>());

            AddSettingPropertyHandler("preferredHeight",
                () => new JValue(m_PreferredHeight.floatValue),
                v => m_PreferredHeight.floatValue = v.Value<float>());

            AddSettingPropertyHandler("flexibleWidth",
                () => new JValue(m_FlexibleWidth.floatValue),
                v => m_FlexibleWidth.floatValue = v.Value<float>());

            AddSettingPropertyHandler("flexibleHeight",
                () => new JValue(m_FlexibleHeight.floatValue),
                v => m_FlexibleHeight.floatValue = v.Value<float>());

            AddSettingPropertyHandler("layoutPriority",
                () => new JValue(m_LayoutPriority.intValue),
                v => m_LayoutPriority.intValue = v.Value<int>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("ignoreLayout", m_IgnoreLayout.boolValue);
            DumpProperty("minWidth", m_MinWidth.floatValue);
            DumpProperty("minHeight", m_MinHeight.floatValue);
            DumpProperty("preferredWidth", m_PreferredWidth.floatValue);
            DumpProperty("preferredHeight", m_PreferredHeight.floatValue);
            DumpProperty("flexibleWidth", m_FlexibleWidth.floatValue);
            DumpProperty("flexibleHeight", m_FlexibleHeight.floatValue);
            DumpProperty("layoutPriority", m_LayoutPriority.intValue);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("LayoutElement", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("ignoreLayout", "boolean"),
                TsPropertyDef.Field("minWidth", "number")
                    .WithComment("-1 means not set"),
                TsPropertyDef.Field("minHeight", "number")
                    .WithComment("-1 means not set"),
                TsPropertyDef.Field("preferredWidth", "number")
                    .WithComment("-1 means not set"),
                TsPropertyDef.Field("preferredHeight", "number")
                    .WithComment("-1 means not set"),
                TsPropertyDef.Field("flexibleWidth", "number")
                    .WithComment("-1 means not set"),
                TsPropertyDef.Field("flexibleHeight", "number")
                    .WithComment("-1 means not set"),
                TsPropertyDef.Field("layoutPriority", "number"),
            });
        }
    }
}
