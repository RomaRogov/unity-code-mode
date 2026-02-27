using System;
using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(CapsuleCollider))]
    public class CapsuleColliderAgentEditor : ColliderAgentEditor
    {
        private SerializedProperty m_Center;
        private SerializedProperty m_Radius;
        private SerializedProperty m_Height;
        private SerializedProperty m_Direction;

        private static readonly string[] DirectionNames = { "X", "Y", "Z" };

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Center = serializedObject.FindProperty("m_Center");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_Height = serializedObject.FindProperty("m_Height");
            m_Direction = serializedObject.FindProperty("m_Direction");

            AddSettingPropertyHandler("center",
                () => m_Center.vector3Value.SerializeToJObject(),
                v => m_Center.vector3Value = v.DeserializeToVector3());

            AddSettingPropertyHandler("radius",
                () => new JValue(m_Radius.floatValue),
                v => m_Radius.floatValue = v.Value<float>());

            AddSettingPropertyHandler("height",
                () => new JValue(m_Height.floatValue),
                v => m_Height.floatValue = v.Value<float>());

            AddSettingPropertyHandler("direction",
                () => new JValue(DirectionNames[m_Direction.intValue]),
                v =>
                {
                    var dir = v.Value<string>();
                    var idx = Array.IndexOf(DirectionNames, dir);
                    if (idx >= 0)
                        m_Direction.intValue = idx;
                    else
                        m_Direction.intValue = v.Value<int>();
                });
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("center", m_Center.vector3Value.SerializeToJObject());
            DumpProperty("radius", m_Radius.floatValue);
            DumpProperty("height", m_Height.floatValue);
            DumpProperty("direction", DirectionNames[m_Direction.intValue]);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            EmitCustomEnumDefinition("CapsuleDirection", new List<KeyValuePair<string, string>>
            {
                new("X", "0"),
                new("Y", "1"),
                new("Z", "2"),
            });

            EmitClassDefinition("CapsuleCollider", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("center", "Vector3"),
                TsPropertyDef.Field("radius", "number"),
                TsPropertyDef.Field("height", "number"),
                TsPropertyDef.Field("direction", "CapsuleDirection"),
            }, "Collider");
        }
    }
}
