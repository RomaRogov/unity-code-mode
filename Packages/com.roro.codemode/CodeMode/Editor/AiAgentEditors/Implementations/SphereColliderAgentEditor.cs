using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(SphereCollider))]
    public class SphereColliderAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_Center;
        private SerializedProperty m_Radius;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Center = serializedObject.FindProperty("m_Center");
            m_Radius = serializedObject.FindProperty("m_Radius");

            AddSettingPropertyHandler("center",
                () => m_Center.vector3Value.SerializeToJObject(),
                v => m_Center.vector3Value = v.DeserializeToVector3());

            AddSettingPropertyHandler("radius",
                () => new JValue(m_Radius.floatValue),
                v => m_Radius.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("center", m_Center.vector3Value.SerializeToJObject());
            DumpProperty("radius", m_Radius.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            EmitClassDefinition("SphereCollider", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("center", "Vector3"),
                TsPropertyDef.Field("radius", "number"),
            }, "Collider");
        }
    }
}
