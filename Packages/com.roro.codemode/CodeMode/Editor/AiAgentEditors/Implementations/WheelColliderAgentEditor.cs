using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(WheelCollider))]
    public class WheelColliderAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_Center;
        private SerializedProperty m_Radius;
        private SerializedProperty m_Mass;
        private SerializedProperty m_SuspensionDistance;
        private SerializedProperty m_ForceAppPointDistance;
        private SerializedProperty m_SuspensionSpring;
        private SerializedProperty m_ForwardFriction;
        private SerializedProperty m_SidewaysFriction;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Center = serializedObject.FindProperty("m_Center");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_Mass = serializedObject.FindProperty("m_Mass");
            m_SuspensionDistance = serializedObject.FindProperty("m_SuspensionDistance");
            m_ForceAppPointDistance = serializedObject.FindProperty("m_ForceAppPointDistance");
            m_SuspensionSpring = serializedObject.FindProperty("m_SuspensionSpring");
            m_ForwardFriction = serializedObject.FindProperty("m_ForwardFriction");
            m_SidewaysFriction = serializedObject.FindProperty("m_SidewaysFriction");

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

            if (m_Mass != null)
            {
                AddSettingPropertyHandler("mass",
                    () => new JValue(m_Mass.floatValue),
                    v => m_Mass.floatValue = v.Value<float>());
            }

            if (m_SuspensionDistance != null)
            {
                AddSettingPropertyHandler("suspensionDistance",
                    () => new JValue(m_SuspensionDistance.floatValue),
                    v => m_SuspensionDistance.floatValue = v.Value<float>());
            }

            if (m_ForceAppPointDistance != null)
            {
                AddSettingPropertyHandler("forceAppPointDistance",
                    () => new JValue(m_ForceAppPointDistance.floatValue),
                    v => m_ForceAppPointDistance.floatValue = v.Value<float>());
            }

            if (m_SuspensionSpring != null)
            {
                AddSettingPropertyHandler("suspensionSpring",
                    () => GetFrictionDump(m_SuspensionSpring),
                    v => SetFriction(m_SuspensionSpring, v));
            }

            if (m_ForwardFriction != null)
            {
                AddSettingPropertyHandler("forwardFriction",
                    () => GetFrictionDump(m_ForwardFriction),
                    v => SetFriction(m_ForwardFriction, v));
            }

            if (m_SidewaysFriction != null)
            {
                AddSettingPropertyHandler("sidewaysFriction",
                    () => GetFrictionDump(m_SidewaysFriction),
                    v => SetFriction(m_SidewaysFriction, v));
            }
        }

        private static string CleanPropertyName(string name)
        {
            if (name.StartsWith("m_"))
                name = name.Substring(2);
            if (name.Length > 0)
                name = char.ToLowerInvariant(name[0]) + name.Substring(1);
            return name;
        }

        private static JObject GetFrictionDump(SerializedProperty prop)
        {
            var result = new JObject();
            var iter = prop.Copy();
            var end = prop.GetEndProperty();
            iter.NextVisible(true);
            while (!SerializedProperty.EqualContents(iter, end))
            {
                if (iter.propertyType == SerializedPropertyType.Float)
                    result[CleanPropertyName(iter.name)] = iter.floatValue;
                if (!iter.NextVisible(false)) break;
            }
            return result;
        }

        private static void SetFriction(SerializedProperty prop, JToken v)
        {
            if (v is not JObject obj) return;
            var iter = prop.Copy();
            var end = prop.GetEndProperty();
            iter.NextVisible(true);
            while (!SerializedProperty.EqualContents(iter, end))
            {
                if (iter.propertyType == SerializedPropertyType.Float &&
                    obj.TryGetValue(CleanPropertyName(iter.name), out var val))
                    iter.floatValue = val.Value<float>();
                if (!iter.NextVisible(false)) break;
            }
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            if (m_Center != null)
                DumpProperty("center", m_Center.vector3Value.SerializeToJObject());
            if (m_Radius != null)
                DumpProperty("radius", m_Radius.floatValue);
            if (m_Mass != null)
                DumpProperty("mass", m_Mass.floatValue);
            if (m_SuspensionDistance != null)
                DumpProperty("suspensionDistance", m_SuspensionDistance.floatValue);
            if (m_ForceAppPointDistance != null)
                DumpProperty("forceAppPointDistance", m_ForceAppPointDistance.floatValue);
            if (m_SuspensionSpring != null)
                DumpProperty("suspensionSpring", GetFrictionDump(m_SuspensionSpring));
            if (m_ForwardFriction != null)
                DumpProperty("forwardFriction", GetFrictionDump(m_ForwardFriction));
            if (m_SidewaysFriction != null)
                DumpProperty("sidewaysFriction", GetFrictionDump(m_SidewaysFriction));
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            EmitClassDefinition("WheelFrictionCurve", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("extremumSlip", "number"),
                TsPropertyDef.Field("extremumValue", "number"),
                TsPropertyDef.Field("asymptoteSlip", "number"),
                TsPropertyDef.Field("asymptoteValue", "number"),
                TsPropertyDef.Field("stiffness", "number"),
            });

            var fields = new List<TsPropertyDef>();
            if (m_Center != null)
                fields.Add(TsPropertyDef.Field("center", "Vector3"));
            if (m_Radius != null)
                fields.Add(TsPropertyDef.Field("radius", "number"));
            if (m_Mass != null)
                fields.Add(TsPropertyDef.Field("mass", "number"));
            if (m_SuspensionDistance != null)
                fields.Add(TsPropertyDef.Field("suspensionDistance", "number"));
            if (m_ForceAppPointDistance != null)
                fields.Add(TsPropertyDef.Field("forceAppPointDistance", "number"));
            if (m_SuspensionSpring != null)
                fields.Add(TsPropertyDef.Field("suspensionSpring", "WheelFrictionCurve"));
            if (m_ForwardFriction != null)
                fields.Add(TsPropertyDef.Field("forwardFriction", "WheelFrictionCurve"));
            if (m_SidewaysFriction != null)
                fields.Add(TsPropertyDef.Field("sidewaysFriction", "WheelFrictionCurve"));

            EmitClassDefinition("WheelCollider", fields, "Collider");
        }
    }
}
