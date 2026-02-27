using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Collider2D))]
    public class Collider2DAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Density;
        private SerializedProperty m_Material;
        private SerializedProperty m_IsTrigger;
        private SerializedProperty m_UsedByEffector;
        private SerializedProperty m_UsedByComposite;
        private SerializedProperty m_Offset;
        private SerializedProperty m_CallbackLayers;

        protected override void OnEnable()
        {
            m_Density = serializedObject.FindProperty("m_Density");
            m_Material = serializedObject.FindProperty("m_Material");
            m_IsTrigger = serializedObject.FindProperty("m_IsTrigger");
            m_UsedByEffector = serializedObject.FindProperty("m_UsedByEffector");
            m_UsedByComposite = serializedObject.FindProperty("m_UsedByComposite");
            m_Offset = serializedObject.FindProperty("m_Offset");
            m_CallbackLayers = serializedObject.FindProperty("m_CallbackLayers");

            if (m_Density != null)
            {
                AddSettingPropertyHandler("density",
                    () => new JValue(m_Density.floatValue),
                    v => m_Density.floatValue = v.Value<float>());
            }

            if (m_Material != null)
            {
                AddSettingPropertyHandler("material",
                    () => SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue),
                    v => SetObjectReferenceWithJTokenInstance(m_Material, v));
            }

            if (m_IsTrigger != null)
            {
                AddSettingPropertyHandler("isTrigger",
                    () => new JValue(m_IsTrigger.boolValue),
                    v => m_IsTrigger.boolValue = v.Value<bool>());
            }

            if (m_UsedByEffector != null)
            {
                AddSettingPropertyHandler("usedByEffector",
                    () => new JValue(m_UsedByEffector.boolValue),
                    v => m_UsedByEffector.boolValue = v.Value<bool>());
            }

            if (m_UsedByComposite != null)
            {
                AddSettingPropertyHandler("usedByComposite",
                    () => new JValue(m_UsedByComposite.boolValue),
                    v => m_UsedByComposite.boolValue = v.Value<bool>());
            }

            if (m_Offset != null)
            {
                AddSettingPropertyHandler("offset",
                    () => m_Offset.vector2Value.SerializeToJObject(),
                    v => m_Offset.vector2Value = v.DeserializeToVector2());
            }

            if (m_CallbackLayers != null)
            {
                AddSettingPropertyHandler("callbackLayers",
                    () => new JValue(m_CallbackLayers.intValue),
                    v => m_CallbackLayers.intValue = v.Value<int>());
            }
        }

        protected override void OnDumpRequested()
        {
            if (m_Density != null)
                DumpProperty("density", m_Density.floatValue);
            if (m_Material != null)
                DumpProperty("material", SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue));
            if (m_IsTrigger != null)
                DumpProperty("isTrigger", m_IsTrigger.boolValue);
            if (m_UsedByEffector != null)
                DumpProperty("usedByEffector", m_UsedByEffector.boolValue);
            if (m_UsedByComposite != null)
                DumpProperty("usedByComposite", m_UsedByComposite.boolValue);
            if (m_Offset != null)
                DumpProperty("offset", m_Offset.vector2Value.SerializeToJObject());
            if (m_CallbackLayers != null)
                DumpProperty("callbackLayers", m_CallbackLayers.intValue);
        }

        protected override void OnDefinitionRequested()
        {
            var fields = new List<TsPropertyDef>();

            if (m_Density != null)
                fields.Add(TsPropertyDef.Field("density", "number"));
            if (m_Material != null)
                fields.Add(TsPropertyDef.Reference("material", "PhysicsMaterial2D").Nullable());
            if (m_IsTrigger != null)
                fields.Add(TsPropertyDef.Field("isTrigger", "boolean"));
            if (m_UsedByEffector != null)
                fields.Add(TsPropertyDef.Field("usedByEffector", "boolean"));
            if (m_UsedByComposite != null)
                fields.Add(TsPropertyDef.Field("usedByComposite", "boolean"));
            if (m_Offset != null)
                fields.Add(TsPropertyDef.Field("offset", "Vector2"));
            if (m_CallbackLayers != null)
                fields.Add(TsPropertyDef.Field("callbackLayers", "number")
                    .WithComment("Layer mask bitmask"));

            EmitClassDefinition("Collider2D", fields);
        }
    }
}
