using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Mask))]
    public class MaskAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_ShowMaskGraphic;

        protected override void OnEnable()
        {
            m_ShowMaskGraphic = serializedObject.FindProperty("m_ShowMaskGraphic");

            AddSettingPropertyHandler("showMaskGraphic",
                () => new JValue(m_ShowMaskGraphic.boolValue),
                v => m_ShowMaskGraphic.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("showMaskGraphic", m_ShowMaskGraphic.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("Mask", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("showMaskGraphic", "boolean"),
            });
        }
    }
}

