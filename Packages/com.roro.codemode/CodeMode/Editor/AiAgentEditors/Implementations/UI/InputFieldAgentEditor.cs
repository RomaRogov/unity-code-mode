using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(InputField))]
    public class InputFieldAgentEditor : SelectableAgentEditor
    {
        private SerializedProperty m_TextComponent;
        private SerializedProperty m_Text;
        private SerializedProperty m_ContentType;
        private SerializedProperty m_LineType;
        private SerializedProperty m_InputType;
        private SerializedProperty m_CharacterValidation;
        private SerializedProperty m_KeyboardType;
        private SerializedProperty m_CharacterLimit;
        private SerializedProperty m_CaretBlinkRate;
        private SerializedProperty m_CaretWidth;
        private SerializedProperty m_CaretColor;
        private SerializedProperty m_CustomCaretColor;
        private SerializedProperty m_SelectionColor;
        private SerializedProperty m_HideMobileInput;
        private SerializedProperty m_Placeholder;
        private SerializedProperty m_ReadOnly;
        private SerializedProperty m_ShouldActivateOnSelect;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_TextComponent = serializedObject.FindProperty("m_TextComponent");
            m_Text = serializedObject.FindProperty("m_Text");
            m_ContentType = serializedObject.FindProperty("m_ContentType");
            m_LineType = serializedObject.FindProperty("m_LineType");
            m_InputType = serializedObject.FindProperty("m_InputType");
            m_CharacterValidation = serializedObject.FindProperty("m_CharacterValidation");
            m_KeyboardType = serializedObject.FindProperty("m_KeyboardType");
            m_CharacterLimit = serializedObject.FindProperty("m_CharacterLimit");
            m_CaretBlinkRate = serializedObject.FindProperty("m_CaretBlinkRate");
            m_CaretWidth = serializedObject.FindProperty("m_CaretWidth");
            m_CaretColor = serializedObject.FindProperty("m_CaretColor");
            m_CustomCaretColor = serializedObject.FindProperty("m_CustomCaretColor");
            m_SelectionColor = serializedObject.FindProperty("m_SelectionColor");
            m_HideMobileInput = serializedObject.FindProperty("m_HideMobileInput");
            m_Placeholder = serializedObject.FindProperty("m_Placeholder");
            m_ReadOnly = serializedObject.FindProperty("m_ReadOnly");
            m_ShouldActivateOnSelect = serializedObject.FindProperty("m_ShouldActivateOnSelect");

            AddSettingPropertyHandler("textComponent",
                () => SerializeInstanceReferenceToJToken(m_TextComponent.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_TextComponent, v));

            AddSettingPropertyHandler("text",
                () => new JValue(m_Text.stringValue),
                v => m_Text.stringValue = v.Value<string>());

            AddSettingPropertyHandler("contentType",
                () => SerializeEnumToJValue(m_ContentType),
                v => SetEnumValue(m_ContentType, v));

            AddSettingPropertyHandler("lineType",
                () => SerializeEnumToJValue(m_LineType),
                v => SetEnumValue(m_LineType, v));

            AddSettingPropertyHandler("inputType",
                () => SerializeEnumToJValue(m_InputType),
                v => SetEnumValue(m_InputType, v));

            AddSettingPropertyHandler("characterValidation",
                () => SerializeEnumToJValue(m_CharacterValidation),
                v => SetEnumValue(m_CharacterValidation, v));

            AddSettingPropertyHandler("keyboardType",
                () => SerializeEnumToJValue(m_KeyboardType),
                v => SetEnumValue(m_KeyboardType, v));

            AddSettingPropertyHandler("characterLimit",
                () => new JValue(m_CharacterLimit.intValue),
                v => m_CharacterLimit.intValue = v.Value<int>());

            AddSettingPropertyHandler("caretBlinkRate",
                () => new JValue(m_CaretBlinkRate.floatValue),
                v => m_CaretBlinkRate.floatValue = v.Value<float>());

            AddSettingPropertyHandler("caretWidth",
                () => new JValue(m_CaretWidth.intValue),
                v => m_CaretWidth.intValue = v.Value<int>());

            AddSettingPropertyHandler("caretColor",
                () => m_CaretColor.colorValue.SerializeToJObject(),
                v => m_CaretColor.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("customCaretColor",
                () => new JValue(m_CustomCaretColor.boolValue),
                v => m_CustomCaretColor.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("selectionColor",
                () => m_SelectionColor.colorValue.SerializeToJObject(),
                v => m_SelectionColor.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("hideMobileInput",
                () => new JValue(m_HideMobileInput.boolValue),
                v => m_HideMobileInput.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("placeholder",
                () => SerializeInstanceReferenceToJToken(m_Placeholder.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Placeholder, v));

            AddSettingPropertyHandler("readOnly",
                () => new JValue(m_ReadOnly.boolValue),
                v => m_ReadOnly.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("shouldActivateOnSelect",
                () => new JValue(m_ShouldActivateOnSelect.boolValue),
                v => m_ShouldActivateOnSelect.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            DumpProperty("textComponent", SerializeInstanceReferenceToJToken(m_TextComponent.objectReferenceValue));
            DumpProperty("text", m_Text.stringValue);
            DumpProperty("contentType", SerializeEnumToJValue(m_ContentType));
            DumpProperty("lineType", SerializeEnumToJValue(m_LineType));
            DumpProperty("inputType", SerializeEnumToJValue(m_InputType));
            DumpProperty("characterValidation", SerializeEnumToJValue(m_CharacterValidation));
            DumpProperty("keyboardType", SerializeEnumToJValue(m_KeyboardType));
            DumpProperty("characterLimit", m_CharacterLimit.intValue);
            DumpProperty("caretBlinkRate", m_CaretBlinkRate.floatValue);
            DumpProperty("caretWidth", m_CaretWidth.intValue);
            DumpProperty("customCaretColor", m_CustomCaretColor.boolValue);
            if (m_CustomCaretColor.boolValue)
                DumpProperty("caretColor", m_CaretColor.colorValue.SerializeToJObject());
            DumpProperty("selectionColor", m_SelectionColor.colorValue.SerializeToJObject());
            DumpProperty("hideMobileInput", m_HideMobileInput.boolValue);
            DumpProperty("placeholder", SerializeInstanceReferenceToJToken(m_Placeholder.objectReferenceValue));
            DumpProperty("readOnly", m_ReadOnly.boolValue);
            DumpProperty("shouldActivateOnSelect", m_ShouldActivateOnSelect.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            GenerateEnumDefinition(typeof(InputField.ContentType));
            GenerateEnumDefinition(typeof(InputField.LineType));
            GenerateEnumDefinition(typeof(InputField.InputType));
            GenerateEnumDefinition(typeof(InputField.CharacterValidation));
            GenerateEnumDefinition(typeof(TouchScreenKeyboardType));

            EmitClassDefinition("InputField", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("textComponent", "Text").Nullable(),
                TsPropertyDef.Field("text", "string"),
                TsPropertyDef.Field("contentType", "ContentType"),
                TsPropertyDef.Field("lineType", "LineType"),
                TsPropertyDef.Field("inputType", "InputType"),
                TsPropertyDef.Field("characterValidation", "CharacterValidation"),
                TsPropertyDef.Field("keyboardType", "TouchScreenKeyboardType"),
                TsPropertyDef.Field("characterLimit", "number"),
                TsPropertyDef.Field("caretBlinkRate", "number"),
                TsPropertyDef.Field("caretWidth", "number"),
                TsPropertyDef.Field("customCaretColor", "boolean"),
                TsPropertyDef.Field("caretColor", "Color"),
                TsPropertyDef.Field("selectionColor", "Color"),
                TsPropertyDef.Field("hideMobileInput", "boolean"),
                TsPropertyDef.Reference("placeholder", "Graphic").Nullable(),
                TsPropertyDef.Field("readOnly", "boolean"),
                TsPropertyDef.Field("shouldActivateOnSelect", "boolean"),
            }, "Selectable");
        }
    }
}


