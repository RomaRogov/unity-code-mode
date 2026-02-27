using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Slider))]
    public class SliderAgentEditor : SelectableAgentEditor
    {
        private SerializedProperty m_FillRect;
        private SerializedProperty m_HandleRect;
        private SerializedProperty m_Direction;
        private SerializedProperty m_MinValue;
        private SerializedProperty m_MaxValue;
        private SerializedProperty m_WholeNumbers;
        private SerializedProperty m_Value;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_FillRect = serializedObject.FindProperty("m_FillRect");
            m_HandleRect = serializedObject.FindProperty("m_HandleRect");
            m_Direction = serializedObject.FindProperty("m_Direction");
            m_MinValue = serializedObject.FindProperty("m_MinValue");
            m_MaxValue = serializedObject.FindProperty("m_MaxValue");
            m_WholeNumbers = serializedObject.FindProperty("m_WholeNumbers");
            m_Value = serializedObject.FindProperty("m_Value");

            AddSettingPropertyHandler("fillRect",
                () => SerializeInstanceReferenceToJToken(m_FillRect.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_FillRect, v));

            AddSettingPropertyHandler("handleRect",
                () => SerializeInstanceReferenceToJToken(m_HandleRect.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_HandleRect, v));

            AddSettingPropertyHandler("direction",
                () => SerializeEnumToJValue(m_Direction),
                v => SetEnumValue(m_Direction, v));

            AddSettingPropertyHandler("minValue",
                () => new JValue(m_MinValue.floatValue),
                v => m_MinValue.floatValue = v.Value<float>());

            AddSettingPropertyHandler("maxValue",
                () => new JValue(m_MaxValue.floatValue),
                v => m_MaxValue.floatValue = v.Value<float>());

            AddSettingPropertyHandler("wholeNumbers",
                () => new JValue(m_WholeNumbers.boolValue),
                v => m_WholeNumbers.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("value",
                () => new JValue(m_Value.floatValue),
                v => m_Value.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            DumpProperty("fillRect", SerializeInstanceReferenceToJToken(m_FillRect.objectReferenceValue));
            DumpProperty("handleRect", SerializeInstanceReferenceToJToken(m_HandleRect.objectReferenceValue));
            DumpProperty("direction", SerializeEnumToJValue(m_Direction));
            DumpProperty("minValue", m_MinValue.floatValue);
            DumpProperty("maxValue", m_MaxValue.floatValue);
            DumpProperty("wholeNumbers", m_WholeNumbers.boolValue);
            DumpProperty("value", m_Value.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            GenerateEnumDefinition(typeof(Slider.Direction));

            EmitClassDefinition("Slider", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("fillRect", "RectTransform").Nullable(),
                TsPropertyDef.Reference("handleRect", "RectTransform").Nullable(),
                TsPropertyDef.Field("direction", "Direction"),
                TsPropertyDef.Field("minValue", "number"),
                TsPropertyDef.Field("maxValue", "number"),
                TsPropertyDef.Field("wholeNumbers", "boolean"),
                TsPropertyDef.Field("value", "number"),
            }, "Selectable");
        }
    }
}

