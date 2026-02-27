using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(TextMeshProUGUI))]
    public class TextMeshProUGUIAgentEditor : TextMeshProAgentEditor
    {
        private SerializedProperty m_RaycastTarget;
        private SerializedProperty m_Maskable;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            m_Maskable = serializedObject.FindProperty("m_Maskable");
            
            AddSettingPropertyHandler("raycastTarget",
                () => new JValue(m_RaycastTarget.boolValue),
                v => m_RaycastTarget.boolValue = v.Value<bool>());
            
            AddSettingPropertyHandler("maskable",
                () => new JValue(m_Maskable.boolValue),
                v => m_Maskable.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            DumpProperty("raycastTarget", m_RaycastTarget.boolValue);
            DumpProperty("maskable", m_Maskable.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            var fields = new List<TsPropertyDef>();
            
            fields.Add(TsPropertyDef.Field("raycastTarget", "boolean"));
            fields.Add(TsPropertyDef.Field("maskable", "boolean"));

            EmitClassDefinition("TextMeshProUGUI", fields, "TextMeshPro");
        }
    }
}

