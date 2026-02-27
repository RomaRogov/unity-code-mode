using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(CharacterController))]
    public class CharacterControllerAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_SlopeLimit;
        private SerializedProperty m_StepOffset;
        private SerializedProperty m_SkinWidth;
        private SerializedProperty m_MinMoveDistance;
        private SerializedProperty m_Center;
        private SerializedProperty m_Radius;
        private SerializedProperty m_Height;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_SlopeLimit = serializedObject.FindProperty("m_SlopeLimit");
            m_StepOffset = serializedObject.FindProperty("m_StepOffset");
            m_SkinWidth = serializedObject.FindProperty("m_SkinWidth");
            m_MinMoveDistance = serializedObject.FindProperty("m_MinMoveDistance");
            m_Center = serializedObject.FindProperty("m_Center");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_Height = serializedObject.FindProperty("m_Height");

            if (m_SlopeLimit != null)
            {
                AddSettingPropertyHandler("slopeLimit",
                    () => new JValue(m_SlopeLimit.floatValue),
                    v => m_SlopeLimit.floatValue = v.Value<float>());
            }

            if (m_StepOffset != null)
            {
                AddSettingPropertyHandler("stepOffset",
                    () => new JValue(m_StepOffset.floatValue),
                    v => m_StepOffset.floatValue = v.Value<float>());
            }

            if (m_SkinWidth != null)
            {
                AddSettingPropertyHandler("skinWidth",
                    () => new JValue(m_SkinWidth.floatValue),
                    v => m_SkinWidth.floatValue = v.Value<float>());
            }

            if (m_MinMoveDistance != null)
            {
                AddSettingPropertyHandler("minMoveDistance",
                    () => new JValue(m_MinMoveDistance.floatValue),
                    v => m_MinMoveDistance.floatValue = v.Value<float>());
            }

            if (m_Center != null)
            {
                AddSettingPropertyHandler("center",
                    () => m_Center.vector3Value.SerializeToJObject(),
                    v => m_Center.vector3Value = v.DeserializeToVector3());
            }

            if (m_Radius != null)
            {
                AddSettingPropertyHandler("radius",
                    () => new JValue(m_Radius.floatValue),
                    v => m_Radius.floatValue = v.Value<float>());
            }

            if (m_Height != null)
            {
                AddSettingPropertyHandler("height",
                    () => new JValue(m_Height.floatValue),
                    v => m_Height.floatValue = v.Value<float>());
            }
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            if (m_SlopeLimit != null)
                DumpProperty("slopeLimit", m_SlopeLimit.floatValue);
            if (m_StepOffset != null)
                DumpProperty("stepOffset", m_StepOffset.floatValue);
            if (m_SkinWidth != null)
                DumpProperty("skinWidth", m_SkinWidth.floatValue);
            if (m_MinMoveDistance != null)
                DumpProperty("minMoveDistance", m_MinMoveDistance.floatValue);
            if (m_Center != null)
                DumpProperty("center", m_Center.vector3Value.SerializeToJObject());
            if (m_Radius != null)
                DumpProperty("radius", m_Radius.floatValue);
            if (m_Height != null)
                DumpProperty("height", m_Height.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            var fields = new List<TsPropertyDef>();
            if (m_SlopeLimit != null)
                fields.Add(TsPropertyDef.Field("slopeLimit", "number")
                    .WithComment("Max slope angle in degrees"));
            if (m_StepOffset != null)
                fields.Add(TsPropertyDef.Field("stepOffset", "number"));
            if (m_SkinWidth != null)
                fields.Add(TsPropertyDef.Field("skinWidth", "number"));
            if (m_MinMoveDistance != null)
                fields.Add(TsPropertyDef.Field("minMoveDistance", "number"));
            if (m_Center != null)
                fields.Add(TsPropertyDef.Field("center", "Vector3"));
            if (m_Radius != null)
                fields.Add(TsPropertyDef.Field("radius", "number"));
            if (m_Height != null)
                fields.Add(TsPropertyDef.Field("height", "number"));

            EmitClassDefinition("CharacterController", fields, "Collider");
        }
    }
}
