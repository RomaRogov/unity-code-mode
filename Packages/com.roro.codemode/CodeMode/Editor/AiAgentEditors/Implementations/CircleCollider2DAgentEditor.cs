using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(CircleCollider2D))]
    public class CircleCollider2DAgentEditor : Collider2DAgentEditor
    {
        private SerializedProperty m_Radius;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Radius = serializedObject.FindProperty("m_Radius");

            AddSettingPropertyHandler("radius",
                () => new JValue(m_Radius.floatValue),
                v => m_Radius.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("radius", m_Radius.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            EmitClassDefinition("CircleCollider2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("radius", "number"),
            }, "Collider2D");
        }
    }
}
