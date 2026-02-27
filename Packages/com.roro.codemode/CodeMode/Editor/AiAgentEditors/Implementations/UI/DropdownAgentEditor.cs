using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Dropdown))]
    public class DropdownAgentEditor : SelectableAgentEditor
    {
        private SerializedProperty m_Template;
        private SerializedProperty m_CaptionText;
        private SerializedProperty m_CaptionImage;
        private SerializedProperty m_ItemText;
        private SerializedProperty m_ItemImage;
        private SerializedProperty m_Value;
        private SerializedProperty m_AlphaFadeSpeed;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_Template = serializedObject.FindProperty("m_Template");
            m_CaptionText = serializedObject.FindProperty("m_CaptionText");
            m_CaptionImage = serializedObject.FindProperty("m_CaptionImage");
            m_ItemText = serializedObject.FindProperty("m_ItemText");
            m_ItemImage = serializedObject.FindProperty("m_ItemImage");
            m_Value = serializedObject.FindProperty("m_Value");
            m_AlphaFadeSpeed = serializedObject.FindProperty("m_AlphaFadeSpeed");

            AddSettingPropertyHandler("template",
                () => SerializeInstanceReferenceToJToken(m_Template.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Template, v));

            AddSettingPropertyHandler("captionText",
                () => SerializeInstanceReferenceToJToken(m_CaptionText.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_CaptionText, v));

            AddSettingPropertyHandler("captionImage",
                () => SerializeInstanceReferenceToJToken(m_CaptionImage.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_CaptionImage, v));

            AddSettingPropertyHandler("itemText",
                () => SerializeInstanceReferenceToJToken(m_ItemText.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_ItemText, v));

            AddSettingPropertyHandler("itemImage",
                () => SerializeInstanceReferenceToJToken(m_ItemImage.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_ItemImage, v));

            AddSettingPropertyHandler("value",
                () => new JValue(m_Value.intValue),
                v => m_Value.intValue = v.Value<int>());

            AddSettingPropertyHandler("alphaFadeSpeed",
                () => new JValue(m_AlphaFadeSpeed.floatValue),
                v => m_AlphaFadeSpeed.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            DumpProperty("template", SerializeInstanceReferenceToJToken(m_Template.objectReferenceValue));
            DumpProperty("captionText", SerializeInstanceReferenceToJToken(m_CaptionText.objectReferenceValue));
            DumpProperty("captionImage", SerializeInstanceReferenceToJToken(m_CaptionImage.objectReferenceValue));
            DumpProperty("itemText", SerializeInstanceReferenceToJToken(m_ItemText.objectReferenceValue));
            DumpProperty("itemImage", SerializeInstanceReferenceToJToken(m_ItemImage.objectReferenceValue));
            DumpProperty("value", m_Value.intValue);
            DumpProperty("alphaFadeSpeed", m_AlphaFadeSpeed.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            EmitClassDefinition("Dropdown", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("template", "RectTransform").Nullable(),
                TsPropertyDef.Reference("captionText", "Text").Nullable(),
                TsPropertyDef.Reference("captionImage", "Image").Nullable(),
                TsPropertyDef.Reference("itemText", "Text").Nullable(),
                TsPropertyDef.Reference("itemImage", "Image").Nullable(),
                TsPropertyDef.Field("value", "number"),
                TsPropertyDef.Field("alphaFadeSpeed", "number"),
            }, "Selectable");
        }
    }
}

