using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Transform))]
    public class TransformAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_LocalPosition;
        private SerializedProperty m_LocalRotation;
        private SerializedProperty m_LocalScale;

        protected override void OnEnable()
        {
            m_LocalPosition = serializedObject.FindProperty("m_LocalPosition");
            m_LocalRotation = serializedObject.FindProperty("m_LocalRotation");
            m_LocalScale = serializedObject.FindProperty("m_LocalScale");
            
            // World position is not directly settable, so we convert it to local position based on the current transform
            AddSettingPropertyHandler("position",
                () => (target as Transform)?.position.SerializeToJObject(),
                v =>
                {
                    if (target is not Transform t) return;
                    var worldPosition = v.DeserializeToVector3();
                    var localPosition = t.parent != null ? t.parent.InverseTransformPoint(worldPosition) : worldPosition;
                    m_LocalPosition.vector3Value = localPosition;
                });

            AddSettingPropertyHandler("localPosition",
                () => m_LocalPosition.vector3Value.SerializeToJObject(),
                v => m_LocalPosition.vector3Value = v.DeserializeToVector3());
            
            // World rotation is not directly settable, so we convert it to local rotation based on the current transform
            AddSettingPropertyHandler("rotation",
                () => (target as Transform)?.rotation.eulerAngles.SerializeToJObject(),
                v =>
                {
                    if (target is not Transform t) return;
                    var worldEuler = v.DeserializeToVector3();
                    var localEuler = worldEuler - (t.parent != null ? t.parent.rotation.eulerAngles : Vector3.zero);
                    m_LocalRotation.quaternionValue = Quaternion.Euler(localEuler);
                });
            
            AddSettingPropertyHandler("localRotation",
                () => m_LocalRotation.quaternionValue.eulerAngles.SerializeToJObject(),
                v => m_LocalRotation.quaternionValue = Quaternion.Euler(v.DeserializeToVector3()));

            AddSettingPropertyHandler("localScale",
                () => m_LocalScale.vector3Value.SerializeToJObject(),
                v => m_LocalScale.vector3Value = v.DeserializeToVector3());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("localPosition", m_LocalPosition.vector3Value.SerializeToJObject());
            DumpProperty("localRotation", m_LocalRotation.quaternionValue.eulerAngles.SerializeToJObject());
            DumpProperty("localScale", m_LocalScale.vector3Value.SerializeToJObject());
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("Transform", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("localPosition", "Vector3"),
                TsPropertyDef.Field("localRotation", "Vector3")
                    .WithComment("Euler angles in degrees"),
                TsPropertyDef.Field("localScale", "Vector3"),
            });
        }
    }
}
