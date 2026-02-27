using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Toggle))]
    public class ToggleAgentEditor : SelectableAgentEditor
    {
        private SerializedProperty m_IsOn;
        private SerializedProperty m_ToggleTransition;
        private SerializedProperty m_Graphic;
        private SerializedProperty m_Group;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_IsOn = serializedObject.FindProperty("m_IsOn");
            m_ToggleTransition = serializedObject.FindProperty("toggleTransition");
            m_Graphic = serializedObject.FindProperty("graphic");
            m_Group = serializedObject.FindProperty("m_Group");

            AddSettingPropertyHandler("isOn",
                () => new JValue(m_IsOn.boolValue),
                v => m_IsOn.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("toggleTransition",
                () => SerializeEnumToJValue(m_ToggleTransition),
                v => SetEnumValue(m_ToggleTransition, v));

            AddSettingPropertyHandler("graphic",
                () => SerializeInstanceReferenceToJToken(m_Graphic.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Graphic, v));

            AddSettingPropertyHandler("group",
                () => SerializeInstanceReferenceToJToken(m_Group.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Group, v));
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("isOn", m_IsOn.boolValue);
            DumpProperty("toggleTransition", SerializeEnumToJValue(m_ToggleTransition));
            DumpProperty("graphic", SerializeInstanceReferenceToJToken(m_Graphic.objectReferenceValue));
            DumpProperty("group", SerializeInstanceReferenceToJToken(m_Group.objectReferenceValue));
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            GenerateEnumDefinition(typeof(Toggle.ToggleTransition));

            EmitClassDefinition("Toggle", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("isOn", "boolean"),
                TsPropertyDef.Field("toggleTransition", "ToggleTransition"),
                TsPropertyDef.Reference("graphic", "Graphic").Nullable(),
                TsPropertyDef.Reference("group", "ToggleGroup").Nullable(),
            }, "Selectable");
        }
    }
}

