using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(BoxCollider))]
    public class BoxColliderAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_Center;
        private SerializedProperty m_Size;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Center = serializedObject.FindProperty("m_Center");
            m_Size = serializedObject.FindProperty("m_Size");

            AddSettingPropertyHandler("center",
                () => m_Center.vector3Value.SerializeToJObject(),
                v => m_Center.vector3Value = v.DeserializeToVector3());

            AddSettingPropertyHandler("size",
                () => m_Size.vector3Value.SerializeToJObject(),
                v => m_Size.vector3Value = v.DeserializeToVector3());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("center", m_Center.vector3Value.SerializeToJObject());
            DumpProperty("size", m_Size.vector3Value.SerializeToJObject());
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            EmitClassDefinition("BoxCollider", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("center", "Vector3"),
                TsPropertyDef.Field("size", "Vector3"),
            }, "Collider");
        }
    }
}
