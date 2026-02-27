using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Rigidbody2D))]
    public class Rigidbody2DAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Simulated;
        private SerializedProperty m_BodyType;
        private SerializedProperty m_Material;
        private SerializedProperty m_UseFullKinematicContacts;
        private SerializedProperty m_UseAutoMass;
        private SerializedProperty m_Mass;
        private SerializedProperty m_LinearDamping;
        private SerializedProperty m_AngularDamping;
        private SerializedProperty m_GravityScale;
        private SerializedProperty m_Interpolate;
        private SerializedProperty m_SleepingMode;
        private SerializedProperty m_CollisionDetection;
        private SerializedProperty m_Constraints;

        protected override void OnEnable()
        {
            m_Simulated = serializedObject.FindProperty("m_Simulated");
            m_BodyType = serializedObject.FindProperty("m_BodyType");
            m_Material = serializedObject.FindProperty("m_Material");
            m_UseFullKinematicContacts = serializedObject.FindProperty("m_UseFullKinematicContacts");
            m_UseAutoMass = serializedObject.FindProperty("m_UseAutoMass");
            m_Mass = serializedObject.FindProperty("m_Mass");
            m_LinearDamping = serializedObject.FindProperty("m_LinearDamping");
            m_AngularDamping = serializedObject.FindProperty("m_AngularDamping");
            m_GravityScale = serializedObject.FindProperty("m_GravityScale");
            m_Interpolate = serializedObject.FindProperty("m_Interpolate");
            m_SleepingMode = serializedObject.FindProperty("m_SleepingMode");
            m_CollisionDetection = serializedObject.FindProperty("m_CollisionDetection");
            m_Constraints = serializedObject.FindProperty("m_Constraints");

            AddSettingPropertyHandler("simulated",
                () => new JValue(m_Simulated.boolValue),
                v => m_Simulated.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("bodyType",
                () => SerializeEnumToJValue(m_BodyType),
                v => SetEnumValue(m_BodyType, v));

            AddSettingPropertyHandler("material",
                () => SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Material, v));

            AddSettingPropertyHandler("useFullKinematicContacts",
                () => new JValue(m_UseFullKinematicContacts.boolValue),
                v => m_UseFullKinematicContacts.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("useAutoMass",
                () => new JValue(m_UseAutoMass.boolValue),
                v => m_UseAutoMass.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("mass",
                () => new JValue(m_Mass.floatValue),
                v => m_Mass.floatValue = v.Value<float>());

            AddSettingPropertyHandler("linearDamping",
                () => new JValue(m_LinearDamping.floatValue),
                v => m_LinearDamping.floatValue = v.Value<float>());

            AddSettingPropertyHandler("angularDamping",
                () => new JValue(m_AngularDamping.floatValue),
                v => m_AngularDamping.floatValue = v.Value<float>());

            AddSettingPropertyHandler("gravityScale",
                () => new JValue(m_GravityScale.floatValue),
                v => m_GravityScale.floatValue = v.Value<float>());

            AddSettingPropertyHandler("interpolate",
                () => SerializeEnumToJValue(m_Interpolate),
                v => SetEnumValue(m_Interpolate, v));

            AddSettingPropertyHandler("sleepingMode",
                () => SerializeEnumToJValue(m_SleepingMode),
                v => SetEnumValue(m_SleepingMode, v));

            AddSettingPropertyHandler("collisionDetectionMode",
                () => SerializeEnumToJValue(m_CollisionDetection),
                v => SetEnumValue(m_CollisionDetection, v));

            AddSettingPropertyHandler("constraints",
                () => SerializeConstraints(),
                v => DeserializeConstraints(v));
        }

        private JObject SerializeConstraints()
        {
            var constraints = (RigidbodyConstraints2D)m_Constraints.intValue;
            return new JObject
            {
                ["freezePositionX"] = (constraints & RigidbodyConstraints2D.FreezePositionX) != 0,
                ["freezePositionY"] = (constraints & RigidbodyConstraints2D.FreezePositionY) != 0,
                ["freezeRotation"] = (constraints & RigidbodyConstraints2D.FreezeRotation) != 0
            };
        }

        private void DeserializeConstraints(JToken v)
        {
            if (v is not JObject obj) return;

            var constraints = RigidbodyConstraints2D.None;

            if (obj.TryGetValue("freezePositionX", out var fpx) && fpx.Value<bool>())
                constraints |= RigidbodyConstraints2D.FreezePositionX;
            if (obj.TryGetValue("freezePositionY", out var fpy) && fpy.Value<bool>())
                constraints |= RigidbodyConstraints2D.FreezePositionY;
            if (obj.TryGetValue("freezeRotation", out var fr) && fr.Value<bool>())
                constraints |= RigidbodyConstraints2D.FreezeRotation;

            m_Constraints.intValue = (int)constraints;
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("simulated", m_Simulated.boolValue);
            
            DumpProperty("bodyType", SerializeEnumToJValue(m_BodyType));
            DumpProperty("material", SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue));
            
            var bodyType = (RigidbodyType2D)m_BodyType.enumValueIndex;
            if (bodyType != RigidbodyType2D.Static)
            {
                if (bodyType != RigidbodyType2D.Kinematic)
                {
                    DumpProperty("useAutoMass", m_UseAutoMass.boolValue);
                    DumpProperty("mass", m_Mass.floatValue);
                    DumpProperty("linearDamping", m_LinearDamping.floatValue);
                    DumpProperty("angularDamping", m_AngularDamping.floatValue);
                    DumpProperty("gravityScale", m_GravityScale.floatValue);
                }
                else
                {
                    DumpProperty("useFullKinematicContacts", m_UseFullKinematicContacts.boolValue);
                }

                DumpProperty("collisionDetectionMode", SerializeEnumToJValue(m_CollisionDetection));
                DumpProperty("sleepingMode", SerializeEnumToJValue(m_SleepingMode));
                DumpProperty("interpolate", SerializeEnumToJValue(m_Interpolate));
                DumpProperty("constraints", SerializeConstraints());
            }
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(RigidbodyType2D));
            GenerateEnumDefinition(typeof(RigidbodyInterpolation2D));
            GenerateEnumDefinition(typeof(RigidbodySleepMode2D));
            GenerateEnumDefinition(typeof(CollisionDetectionMode2D));

            EmitClassDefinition("RigidbodyConstraints2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("freezePositionX", "boolean"),
                TsPropertyDef.Field("freezePositionY", "boolean"),
                TsPropertyDef.Field("freezeRotation", "boolean"),
            });

            EmitClassDefinition("Rigidbody2D", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("simulated", "boolean"),
                TsPropertyDef.Field("bodyType", "RigidbodyType2D"),
                TsPropertyDef.Reference("material", "PhysicsMaterial2D").Nullable(),
                TsPropertyDef.Field("useFullKinematicContacts", "boolean"),
                TsPropertyDef.Field("useAutoMass", "boolean"),
                TsPropertyDef.Field("mass", "number"),
                TsPropertyDef.Field("linearDamping", "number"),
                TsPropertyDef.Field("angularDamping", "number"),
                TsPropertyDef.Field("gravityScale", "number"),
                TsPropertyDef.Field("interpolate", "RigidbodyInterpolation2D"),
                TsPropertyDef.Field("sleepingMode", "RigidbodySleepMode2D"),
                TsPropertyDef.Field("collisionDetectionMode", "CollisionDetectionMode2D"),
                TsPropertyDef.Field("constraints", "RigidbodyConstraints2D")
            });
        }
    }
}


