using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(ToggleGroup))]
    public class ToggleGroupAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_AllowSwitchOff;

        protected override void OnEnable()
        {
            m_AllowSwitchOff = serializedObject.FindProperty("m_AllowSwitchOff");

            AddSettingPropertyHandler("allowSwitchOff",
                () => new JValue(m_AllowSwitchOff.boolValue),
                v => m_AllowSwitchOff.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("allowSwitchOff", m_AllowSwitchOff.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("ToggleGroup", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("allowSwitchOff", "boolean"),
            });
        }
    }
}

