using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(TextMeshPro))]
    public class TextMeshProAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Text;
        private SerializedProperty m_FontAsset;
        private SerializedProperty m_SharedMaterial;
        private SerializedProperty m_FontSize;
        private SerializedProperty m_EnableAutoSizing;
        private SerializedProperty m_FontSizeMin;
        private SerializedProperty m_FontSizeMax;
        private SerializedProperty m_FontColor;
        private SerializedProperty m_HorizontalAlignment;
        private SerializedProperty m_VerticalAlignment;
        private SerializedProperty m_EnableWordWrapping;
        private SerializedProperty m_OverflowMode;
        private SerializedProperty m_Margin;

        protected override void OnEnable()
        {
            m_Text = serializedObject.FindProperty("m_text");
            m_FontAsset = serializedObject.FindProperty("m_fontAsset");
            m_SharedMaterial = serializedObject.FindProperty("m_sharedMaterial");
            m_FontSize = serializedObject.FindProperty("m_fontSize");
            m_EnableAutoSizing = serializedObject.FindProperty("m_enableAutoSizing");
            m_FontSizeMin = serializedObject.FindProperty("m_fontSizeMin");
            m_FontSizeMax = serializedObject.FindProperty("m_fontSizeMax");
            m_FontColor = serializedObject.FindProperty("m_fontColor");
            m_HorizontalAlignment = serializedObject.FindProperty("m_HorizontalAlignment");
            m_VerticalAlignment = serializedObject.FindProperty("m_VerticalAlignment");
            m_EnableWordWrapping = serializedObject.FindProperty("m_enableWordWrapping");
            m_OverflowMode = serializedObject.FindProperty("m_overflowMode");
            m_Margin = serializedObject.FindProperty("m_margin");

            AddSettingPropertyHandler("text",
                () => new JValue(m_Text.stringValue),
                v => m_Text.stringValue = v.Value<string>());

            AddSettingPropertyHandler("fontAsset",
                () => SerializeInstanceReferenceToJToken(m_FontAsset.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_FontAsset, v));

            AddSettingPropertyHandler("sharedMaterial",
                () => SerializeInstanceReferenceToJToken(m_SharedMaterial.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_SharedMaterial, v));

            AddSettingPropertyHandler("fontSize",
                () => new JValue(m_FontSize.floatValue),
                v => m_FontSize.floatValue = v.Value<float>());

            AddSettingPropertyHandler("enableAutoSizing",
                () => new JValue(m_EnableAutoSizing.boolValue),
                v => m_EnableAutoSizing.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("fontSizeMin",
                () => new JValue(m_FontSizeMin.floatValue),
                v => m_FontSizeMin.floatValue = v.Value<float>());

            AddSettingPropertyHandler("fontSizeMax",
                () => new JValue(m_FontSizeMax.floatValue),
                v => m_FontSizeMax.floatValue = v.Value<float>());

            AddSettingPropertyHandler("fontColor",
                () => m_FontColor.colorValue.SerializeToJObject(),
                v => m_FontColor.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("horizontalAlignment",
                () => SerializeEnumToJValue(m_HorizontalAlignment),
                v => SetEnumValue(m_HorizontalAlignment, v));

            AddSettingPropertyHandler("verticalAlignment",
                () => SerializeEnumToJValue(m_VerticalAlignment),
                v => SetEnumValue(m_VerticalAlignment, v));

            AddSettingPropertyHandler("enableWordWrapping",
                () => new JValue(m_EnableWordWrapping.boolValue),
                v => m_EnableWordWrapping.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("overflowMode",
                () => SerializeEnumToJValue(m_OverflowMode),
                v => SetEnumValue(m_OverflowMode, v));

            AddSettingPropertyHandler("margin",
                () => m_Margin.vector4Value.SerializeToJObject(),
                v => m_Margin.vector4Value = v.DeserializeToVector4());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("text", m_Text.stringValue);
            DumpProperty("fontAsset", SerializeInstanceReferenceToJToken(m_FontAsset.objectReferenceValue));
            DumpProperty("sharedMaterial", SerializeInstanceReferenceToJToken(m_SharedMaterial.objectReferenceValue));
            DumpProperty("fontSize", m_FontSize.floatValue);
            DumpProperty("enableAutoSizing", m_EnableAutoSizing.boolValue);
            
            if (m_EnableAutoSizing.boolValue)
            {
                DumpProperty("fontSizeMin", m_FontSizeMin.floatValue);
                DumpProperty("fontSizeMax", m_FontSizeMax.floatValue);
            }
            
            DumpProperty("fontColor", m_FontColor.colorValue.SerializeToJObject());
            DumpProperty("horizontalAlignment", SerializeEnumToJValue(m_HorizontalAlignment));
            DumpProperty("verticalAlignment", SerializeEnumToJValue(m_VerticalAlignment));
            DumpProperty("enableWordWrapping", m_EnableWordWrapping.boolValue);
            DumpProperty("overflowMode", SerializeEnumToJValue(m_OverflowMode));
            DumpProperty("margin", m_Margin.vector4Value.SerializeToJObject());
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(HorizontalAlignmentOptions));
            GenerateEnumDefinition(typeof(VerticalAlignmentOptions));
            GenerateEnumDefinition(typeof(TextOverflowModes));

            EmitClassDefinition("TextMeshPro", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("text", "string"),
                TsPropertyDef.Reference("fontAsset", "TMP_FontAsset").Nullable(),
                TsPropertyDef.Reference("sharedMaterial", "Material").Nullable(),
                TsPropertyDef.Field("fontSize", "number"),
                TsPropertyDef.Field("enableAutoSizing", "boolean"),
                TsPropertyDef.Field("fontSizeMin", "number"),
                TsPropertyDef.Field("fontSizeMax", "number"),
                TsPropertyDef.Field("fontColor", "Color"),
                TsPropertyDef.Field("horizontalAlignment", "HorizontalAlignmentOptions"),
                TsPropertyDef.Field("verticalAlignment", "VerticalAlignmentOptions"),
                TsPropertyDef.Field("enableWordWrapping", "boolean"),
                TsPropertyDef.Field("overflowMode", "TextOverflowModes"),
                TsPropertyDef.Field("margin", "Vector4")
            });
        }
    }
}



