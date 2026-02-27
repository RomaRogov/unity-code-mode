using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Rigidbody))]
    public class RigidbodyAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Mass;
        private SerializedProperty m_LinearDamping;
        private SerializedProperty m_AngularDamping;
        private SerializedProperty m_CenterOfMass;
        private SerializedProperty m_InertiaTensor;
        private SerializedProperty m_InertiaRotation;
        private SerializedProperty m_ImplicitCom;
        private SerializedProperty m_ImplicitTensor;
        private SerializedProperty m_UseGravity;
        private SerializedProperty m_IsKinematic;
        private SerializedProperty m_Interpolate;
        private SerializedProperty m_CollisionDetection;
        private SerializedProperty m_Constraints;

        protected override void OnEnable()
        {
            m_Mass = serializedObject.FindProperty("m_Mass");
            
            m_LinearDamping = serializedObject.FindProperty("m_LinearDamping");
            if (m_LinearDamping == null)
                m_LinearDamping = serializedObject.FindProperty("m_Drag");
            
            m_AngularDamping = serializedObject.FindProperty("m_AngularDamping");
            if (m_AngularDamping == null)
                m_AngularDamping = serializedObject.FindProperty("m_AngularDrag");
            
            m_CenterOfMass = serializedObject.FindProperty("m_CenterOfMass");
            m_InertiaTensor = serializedObject.FindProperty("m_InertiaTensor");
            m_InertiaRotation = serializedObject.FindProperty("m_InertiaRotation");
            m_ImplicitCom = serializedObject.FindProperty("m_ImplicitCom");
            m_ImplicitTensor = serializedObject.FindProperty("m_ImplicitTensor");
            m_UseGravity = serializedObject.FindProperty("m_UseGravity");
            m_IsKinematic = serializedObject.FindProperty("m_IsKinematic");
            m_Interpolate = serializedObject.FindProperty("m_Interpolate");
            m_CollisionDetection = serializedObject.FindProperty("m_CollisionDetection");
            m_Constraints = serializedObject.FindProperty("m_Constraints");

            AddSettingPropertyHandler("mass",
                () => new JValue(m_Mass.floatValue),
                v => m_Mass.floatValue = v.Value<float>());

            AddSettingPropertyHandler("linearDamping",
                () => new JValue(m_LinearDamping.floatValue),
                v => m_LinearDamping.floatValue = v.Value<float>());

            AddSettingPropertyHandler("angularDamping",
                () => new JValue(m_AngularDamping.floatValue),
                v => m_AngularDamping.floatValue = v.Value<float>());

            AddSettingPropertyHandler("useGravity",
                () => new JValue(m_UseGravity.boolValue),
                v => m_UseGravity.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("isKinematic",
                () => new JValue(m_IsKinematic.boolValue),
                v => m_IsKinematic.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("automaticCenterOfMass",
                () => new JValue(m_ImplicitCom.boolValue),
                v => m_ImplicitCom.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("automaticTensor",
                () => new JValue(m_ImplicitTensor.boolValue),
                v => m_ImplicitTensor.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("interpolate",
                () => SerializeEnumToJValue(m_Interpolate),
                v => SetEnumValue(m_Interpolate, v));

            AddSettingPropertyHandler("collisionDetectionMode",
                () => SerializeEnumToJValue(m_CollisionDetection),
                v => SetEnumValue(m_CollisionDetection, v));

            AddSettingPropertyHandler("constraints",
                () => SerializeConstraints(),
                v => DeserializeConstraints(v));

            AddSettingPropertyHandler("centerOfMass",
                () => m_CenterOfMass.vector3Value.SerializeToJObject(),
                v => m_CenterOfMass.vector3Value = v.DeserializeToVector3());

            AddSettingPropertyHandler("inertiaTensor",
                () => m_InertiaTensor.vector3Value.SerializeToJObject(),
                v => m_InertiaTensor.vector3Value = v.DeserializeToVector3());

            AddSettingPropertyHandler("inertiaRotation",
                () => m_InertiaRotation.quaternionValue.eulerAngles.SerializeToJObject(),
                v => m_InertiaRotation.quaternionValue = Quaternion.Euler(v.DeserializeToVector3()));
        }

        private JObject SerializeConstraints()
        {
            var constraints = (RigidbodyConstraints)m_Constraints.intValue;
            return new JObject
            {
                ["freezePositionX"] = (constraints & RigidbodyConstraints.FreezePositionX) != 0,
                ["freezePositionY"] = (constraints & RigidbodyConstraints.FreezePositionY) != 0,
                ["freezePositionZ"] = (constraints & RigidbodyConstraints.FreezePositionZ) != 0,
                ["freezeRotationX"] = (constraints & RigidbodyConstraints.FreezeRotationX) != 0,
                ["freezeRotationY"] = (constraints & RigidbodyConstraints.FreezeRotationY) != 0,
                ["freezeRotationZ"] = (constraints & RigidbodyConstraints.FreezeRotationZ) != 0
            };
        }

        private void DeserializeConstraints(JToken v)
        {
            if (v is not JObject obj) return;

            var constraints = RigidbodyConstraints.None;

            if (obj.TryGetValue("freezePositionX", out var fpx) && fpx.Value<bool>())
                constraints |= RigidbodyConstraints.FreezePositionX;
            if (obj.TryGetValue("freezePositionY", out var fpy) && fpy.Value<bool>())
                constraints |= RigidbodyConstraints.FreezePositionY;
            if (obj.TryGetValue("freezePositionZ", out var fpz) && fpz.Value<bool>())
                constraints |= RigidbodyConstraints.FreezePositionZ;
            if (obj.TryGetValue("freezeRotationX", out var frx) && frx.Value<bool>())
                constraints |= RigidbodyConstraints.FreezeRotationX;
            if (obj.TryGetValue("freezeRotationY", out var fry) && fry.Value<bool>())
                constraints |= RigidbodyConstraints.FreezeRotationY;
            if (obj.TryGetValue("freezeRotationZ", out var frz) && frz.Value<bool>())
                constraints |= RigidbodyConstraints.FreezeRotationZ;

            m_Constraints.intValue = (int)constraints;
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("mass", m_Mass.floatValue);
            DumpProperty("linearDamping", m_LinearDamping.floatValue);
            DumpProperty("angularDamping", m_AngularDamping.floatValue);
            DumpProperty("useGravity", m_UseGravity.boolValue);
            DumpProperty("isKinematic", m_IsKinematic.boolValue);
            DumpProperty("automaticCenterOfMass", m_ImplicitCom.boolValue);
            DumpProperty("automaticTensor", m_ImplicitTensor.boolValue);
            DumpProperty("interpolate", SerializeEnumToJValue(m_Interpolate));
            DumpProperty("collisionDetectionMode", SerializeEnumToJValue(m_CollisionDetection));
            DumpProperty("constraints", SerializeConstraints());

            if (!m_ImplicitCom.boolValue)
                DumpProperty("centerOfMass", m_CenterOfMass.vector3Value.SerializeToJObject());

            if (!m_ImplicitTensor.boolValue)
            {
                DumpProperty("inertiaTensor", m_InertiaTensor.vector3Value.SerializeToJObject());
                DumpProperty("inertiaRotation", m_InertiaRotation.quaternionValue.eulerAngles.SerializeToJObject());
            }
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(RigidbodyInterpolation));
            GenerateEnumDefinition(typeof(CollisionDetectionMode));

            EmitClassDefinition("RigidbodyConstraints", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("freezePositionX", "boolean"),
                TsPropertyDef.Field("freezePositionY", "boolean"),
                TsPropertyDef.Field("freezePositionZ", "boolean"),
                TsPropertyDef.Field("freezeRotationX", "boolean"),
                TsPropertyDef.Field("freezeRotationY", "boolean"),
                TsPropertyDef.Field("freezeRotationZ", "boolean"),
            });

            EmitClassDefinition("Rigidbody", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("mass", "number"),
                TsPropertyDef.Field("linearDamping", "number"),
                TsPropertyDef.Field("angularDamping", "number"),
                TsPropertyDef.Field("useGravity", "boolean"),
                TsPropertyDef.Field("isKinematic", "boolean"),
                TsPropertyDef.Field("interpolate", "RigidbodyInterpolation"),
                TsPropertyDef.Field("collisionDetectionMode", "CollisionDetectionMode"),
                TsPropertyDef.Field("constraints", "RigidbodyConstraints"),
                TsPropertyDef.Field("automaticCenterOfMass", "boolean"),
                TsPropertyDef.Field("automaticTensor", "boolean"),
                TsPropertyDef.Field("centerOfMass", "Vector3"),
                TsPropertyDef.Field("inertiaTensor", "Vector3"),
                TsPropertyDef.Field("inertiaRotation", "Vector3"),
            });
        }
    }
}





