using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(HorizontalOrVerticalLayoutGroup))]
    public class HorizontalOrVerticalLayoutGroupAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Padding;
        private SerializedProperty m_ChildAlignment;
        private SerializedProperty m_Spacing;
        private SerializedProperty m_ChildControlWidth;
        private SerializedProperty m_ChildControlHeight;
        private SerializedProperty m_ChildScaleWidth;
        private SerializedProperty m_ChildScaleHeight;
        private SerializedProperty m_ChildForceExpandWidth;
        private SerializedProperty m_ChildForceExpandHeight;
        private SerializedProperty m_ReverseArrangement;

        protected override void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");
            m_ChildScaleWidth = serializedObject.FindProperty("m_ChildScaleWidth");
            m_ChildScaleHeight = serializedObject.FindProperty("m_ChildScaleHeight");
            m_ChildForceExpandWidth = serializedObject.FindProperty("m_ChildForceExpandWidth");
            m_ChildForceExpandHeight = serializedObject.FindProperty("m_ChildForceExpandHeight");
            m_ReverseArrangement = serializedObject.FindProperty("m_ReverseArrangement");

            AddSettingPropertyHandler("padding",
                () => GetPaddingDump(),
                v => SetPadding(v));

            AddSettingPropertyHandler("childAlignment",
                () => SerializeEnumToJValue(m_ChildAlignment),
                v => SetEnumValue(m_ChildAlignment, v));

            AddSettingPropertyHandler("spacing",
                () => new JValue(m_Spacing.floatValue),
                v => m_Spacing.floatValue = v.Value<float>());

            AddSettingPropertyHandler("childControlWidth",
                () => new JValue(m_ChildControlWidth.boolValue),
                v => m_ChildControlWidth.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("childControlHeight",
                () => new JValue(m_ChildControlHeight.boolValue),
                v => m_ChildControlHeight.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("childScaleWidth",
                () => new JValue(m_ChildScaleWidth.boolValue),
                v => m_ChildScaleWidth.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("childScaleHeight",
                () => new JValue(m_ChildScaleHeight.boolValue),
                v => m_ChildScaleHeight.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("childForceExpandWidth",
                () => new JValue(m_ChildForceExpandWidth.boolValue),
                v => m_ChildForceExpandWidth.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("childForceExpandHeight",
                () => new JValue(m_ChildForceExpandHeight.boolValue),
                v => m_ChildForceExpandHeight.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("reverseArrangement",
                () => new JValue(m_ReverseArrangement.boolValue),
                v => m_ReverseArrangement.boolValue = v.Value<bool>());
        }

        private JObject GetPaddingDump()
        {
            return new JObject
            {
                ["left"] = m_Padding.FindPropertyRelative("m_Left").intValue,
                ["right"] = m_Padding.FindPropertyRelative("m_Right").intValue,
                ["top"] = m_Padding.FindPropertyRelative("m_Top").intValue,
                ["bottom"] = m_Padding.FindPropertyRelative("m_Bottom").intValue
            };
        }

        private void SetPadding(JToken v)
        {
            if (v is JObject obj)
            {
                if (obj.TryGetValue("left", out var left))
                    m_Padding.FindPropertyRelative("m_Left").intValue = left.Value<int>();
                if (obj.TryGetValue("right", out var right))
                    m_Padding.FindPropertyRelative("m_Right").intValue = right.Value<int>();
                if (obj.TryGetValue("top", out var top))
                    m_Padding.FindPropertyRelative("m_Top").intValue = top.Value<int>();
                if (obj.TryGetValue("bottom", out var bottom))
                    m_Padding.FindPropertyRelative("m_Bottom").intValue = bottom.Value<int>();
            }
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("padding", GetPaddingDump());
            DumpProperty("childAlignment", SerializeEnumToJValue(m_ChildAlignment));
            DumpProperty("spacing", m_Spacing.floatValue);
            DumpProperty("childControlWidth", m_ChildControlWidth.boolValue);
            DumpProperty("childControlHeight", m_ChildControlHeight.boolValue);
            DumpProperty("childScaleWidth", m_ChildScaleWidth.boolValue);
            DumpProperty("childScaleHeight", m_ChildScaleHeight.boolValue);
            DumpProperty("childForceExpandWidth", m_ChildForceExpandWidth.boolValue);
            DumpProperty("childForceExpandHeight", m_ChildForceExpandHeight.boolValue);
            DumpProperty("reverseArrangement", m_ReverseArrangement.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(TextAnchor));

            EmitClassDefinition("RectOffsetPadding", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("left", "number"),
                TsPropertyDef.Field("right", "number"),
                TsPropertyDef.Field("top", "number"),
                TsPropertyDef.Field("bottom", "number"),
            });

            EmitClassDefinition(serializedObject.targetObject.GetType().Name, new List<TsPropertyDef>
            {
                TsPropertyDef.Field("padding", "RectOffsetPadding"),
                TsPropertyDef.Field("childAlignment", "TextAnchor"),
                TsPropertyDef.Field("spacing", "number"),
                TsPropertyDef.Field("childControlWidth", "boolean"),
                TsPropertyDef.Field("childControlHeight", "boolean"),
                TsPropertyDef.Field("childScaleWidth", "boolean"),
                TsPropertyDef.Field("childScaleHeight", "boolean"),
                TsPropertyDef.Field("childForceExpandWidth", "boolean"),
                TsPropertyDef.Field("childForceExpandHeight", "boolean"),
                TsPropertyDef.Field("reverseArrangement", "boolean"),
            });
        }
    }
}
