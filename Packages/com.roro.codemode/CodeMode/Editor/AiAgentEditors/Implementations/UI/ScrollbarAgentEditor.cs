using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Scrollbar))]
    public class ScrollbarAgentEditor : SelectableAgentEditor
    {
        private SerializedProperty m_HandleRect;
        private SerializedProperty m_Direction;
        private SerializedProperty m_Value;
        private SerializedProperty m_Size;
        private SerializedProperty m_NumberOfSteps;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_HandleRect = serializedObject.FindProperty("m_HandleRect");
            m_Direction = serializedObject.FindProperty("m_Direction");
            m_Value = serializedObject.FindProperty("m_Value");
            m_Size = serializedObject.FindProperty("m_Size");
            m_NumberOfSteps = serializedObject.FindProperty("m_NumberOfSteps");

            AddSettingPropertyHandler("handleRect",
                () => SerializeInstanceReferenceToJToken(m_HandleRect.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_HandleRect, v));

            AddSettingPropertyHandler("direction",
                () => SerializeEnumToJValue(m_Direction),
                v => SetEnumValue(m_Direction, v));

            AddSettingPropertyHandler("value",
                () => new JValue(m_Value.floatValue),
                v => m_Value.floatValue = v.Value<float>());

            AddSettingPropertyHandler("size",
                () => new JValue(m_Size.floatValue),
                v => m_Size.floatValue = v.Value<float>());

            AddSettingPropertyHandler("numberOfSteps",
                () => new JValue(m_NumberOfSteps.intValue),
                v => m_NumberOfSteps.intValue = v.Value<int>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            DumpProperty("handleRect", SerializeInstanceReferenceToJToken(m_HandleRect.objectReferenceValue));
            DumpProperty("direction", SerializeEnumToJValue(m_Direction));
            DumpProperty("value", m_Value.floatValue);
            DumpProperty("size", m_Size.floatValue);
            DumpProperty("numberOfSteps", m_NumberOfSteps.intValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            GenerateEnumDefinition(typeof(Scrollbar.Direction));

            EmitClassDefinition("Scrollbar", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("handleRect", "RectTransform").Nullable(),
                TsPropertyDef.Field("direction", "Direction"),
                TsPropertyDef.Field("value", "number"),
                TsPropertyDef.Field("size", "number"),
                TsPropertyDef.Field("numberOfSteps", "number"),
            }, "Selectable");
        }
    }
}

