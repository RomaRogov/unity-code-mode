using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Collider))]
    public class ColliderAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_IsTrigger;
        private SerializedProperty m_Material;
        private SerializedProperty m_ProvidesContacts;

        protected override void OnEnable()
        {
            m_IsTrigger = serializedObject.FindProperty("m_IsTrigger");
            m_Material = serializedObject.FindProperty("m_Material");
            m_ProvidesContacts = serializedObject.FindProperty("m_ProvidesContacts");

            if (m_IsTrigger != null)
            {
                AddSettingPropertyHandler("isTrigger",
                    () => new JValue(m_IsTrigger.boolValue),
                    v => m_IsTrigger.boolValue = v.Value<bool>());
            }

            if (m_Material != null)
            {
                AddSettingPropertyHandler("material",
                    () => SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue),
                    v => SetObjectReferenceWithJTokenInstance(m_Material, v));
            }

            if (m_ProvidesContacts != null)
            {
                AddSettingPropertyHandler("providesContacts",
                    () => new JValue(m_ProvidesContacts.boolValue),
                    v => m_ProvidesContacts.boolValue = v.Value<bool>());
            }
        }

        protected override void OnDumpRequested()
        {
            if (m_IsTrigger != null)
                DumpProperty("isTrigger", m_IsTrigger.boolValue);
            if (m_Material != null)
                DumpProperty("material", SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue));
            if (m_ProvidesContacts != null)
                DumpProperty("providesContacts", m_ProvidesContacts.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            var fields = new List<TsPropertyDef>();

            if (m_IsTrigger != null)
                fields.Add(TsPropertyDef.Field("isTrigger", "boolean"));
            if (m_Material != null)
                fields.Add(TsPropertyDef.Reference("material", "PhysicMaterial").Nullable());
            if (m_ProvidesContacts != null)
                fields.Add(TsPropertyDef.Field("providesContacts", "boolean"));

            EmitClassDefinition("Collider", fields);
        }
    }
}
