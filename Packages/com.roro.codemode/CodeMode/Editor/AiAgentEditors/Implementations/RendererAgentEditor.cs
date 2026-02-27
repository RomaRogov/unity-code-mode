using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Renderer))]
    public class RendererAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_CastShadows;
        private SerializedProperty m_ReceiveShadows;
        private SerializedProperty m_DynamicOccludee;
        private SerializedProperty m_StaticShadowCaster;
        private SerializedProperty m_MotionVectors;
        private SerializedProperty m_Materials;

        protected override void OnEnable()
        {
            m_CastShadows = serializedObject.FindProperty("m_CastShadows");
            m_ReceiveShadows = serializedObject.FindProperty("m_ReceiveShadows");
            m_DynamicOccludee = serializedObject.FindProperty("m_DynamicOccludee");
            m_StaticShadowCaster = serializedObject.FindProperty("m_StaticShadowCaster");
            m_MotionVectors = serializedObject.FindProperty("m_MotionVectors");
            m_Materials = serializedObject.FindProperty("m_Materials");

            if (m_CastShadows != null)
            {
                AddSettingPropertyHandler("castShadows",
                    () => m_CastShadows.enumNames[m_CastShadows.intValue],
                    v => SetEnumValue(m_CastShadows, v));
            }

            if (m_ReceiveShadows != null)
            {
                AddSettingPropertyHandler("receiveShadows",
                    () => new JValue(m_ReceiveShadows.boolValue),
                    v => m_ReceiveShadows.boolValue = v.Value<bool>());
            }

            if (m_DynamicOccludee != null)
            {
                AddSettingPropertyHandler("dynamicOccludee",
                    () => new JValue(m_DynamicOccludee.boolValue),
                    v => m_DynamicOccludee.boolValue = v.Value<bool>());
            }

            if (m_StaticShadowCaster != null)
            {
                AddSettingPropertyHandler("staticShadowCaster",
                    () => new JValue(m_StaticShadowCaster.boolValue),
                    v => m_StaticShadowCaster.boolValue = v.Value<bool>());
            }

            if (m_MotionVectors != null)
            {
                AddSettingPropertyHandler("motionVectors",
                    () => m_MotionVectors.enumNames[m_MotionVectors.intValue],
                    v => SetEnumValue(m_MotionVectors, v));
            }

            if (m_Materials != null)
            {
                AddSettingPropertyHandler("sharedMaterials",
                    () =>
                    {
                        var arr = new JArray();
                        for (int i = 0; i < m_Materials.arraySize; i++)
                        {
                            var element = m_Materials.GetArrayElementAtIndex(i);
                            arr.Add(SerializeInstanceReferenceToJToken(element.objectReferenceValue));
                        }
                        return arr;
                    },
                    v =>
                    {
                        if (v is not JArray arr)
                            return;

                        m_Materials.ClearArray();
                        for (int i = 0; i < arr.Count; i++)
                        {
                            m_Materials.InsertArrayElementAtIndex(i);
                            var element = m_Materials.GetArrayElementAtIndex(i);
                            SetObjectReferenceWithJTokenInstance(element, arr[i]);
                        }
                    });

                AddSettingPropertyHandler("sharedMaterial",
                    () =>
                    {
                        if (m_Materials.arraySize > 0)
                        {
                            var element = m_Materials.GetArrayElementAtIndex(0);
                            return SerializeInstanceReferenceToJToken(element.objectReferenceValue);
                        }
                        return null;
                    },
                    v =>
                    {
                        if (v == null)
                            return;

                        if (m_Materials.arraySize == 0)
                            m_Materials.InsertArrayElementAtIndex(0);

                        var element = m_Materials.GetArrayElementAtIndex(0);
                        SetObjectReferenceWithJTokenInstance(element, v);
                    });
            }
        }

        protected override void OnDumpRequested()
        {
            if (m_CastShadows != null)
                DumpProperty("castShadows", m_CastShadows.enumNames[m_CastShadows.enumValueIndex]);

            if (m_ReceiveShadows != null)
                DumpProperty("receiveShadows", m_ReceiveShadows.boolValue);

            if (m_DynamicOccludee != null)
                DumpProperty("dynamicOccludee", m_DynamicOccludee.boolValue);

            if (m_StaticShadowCaster != null)
                DumpProperty("staticShadowCaster", m_StaticShadowCaster.boolValue);

            if (m_MotionVectors != null)
                DumpProperty("motionVectors", m_MotionVectors.enumNames[m_MotionVectors.intValue]);

            if (m_Materials != null)
            {
                var arr = new JArray();
                for (int i = 0; i < m_Materials.arraySize; i++)
                {
                    var element = m_Materials.GetArrayElementAtIndex(i);
                    arr.Add(SerializeInstanceReferenceToJToken(element.objectReferenceValue));
                }
                DumpProperty("sharedMaterials", arr);
                if (m_Materials.arraySize > 0)
                {
                    DumpProperty("sharedMaterial",
                        SerializeInstanceReferenceToJToken(m_Materials.GetArrayElementAtIndex(0).objectReferenceValue));
                }
            }
        }

        protected override void OnDefinitionRequested()
        {
            EmitCustomEnumDefinition("ShadowCastingMode", new List<KeyValuePair<string, string>>
            {
                new("Off", "Off"),
                new("On", "On"),
                new("TwoSided", "Two Sided"),
                new("ShadowsOnly", "Shadows Only"),
            });

            EmitCustomEnumDefinition("MotionVectorGenerationMode", new List<KeyValuePair<string, string>>
            {
                new("CameraMotionOnly", "Camera Motion Only"),
                new("PerObjectMotion", "Per Object Motion"),
                new("ForceNoMotion", "Force No Motion"),
            });

            EmitClassDefinition("Renderer", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("sharedMaterial", "InstanceReference<Material>")
                    .WithComment("First material used by this renderer"),
                TsPropertyDef.Field("sharedMaterials", "Array<InstanceReference<Material>>")
                    .WithComment("Array of materials used by this renderer"),
                TsPropertyDef.Field("castShadows", "ShadowCastingMode")
                    .WithComment("Shadow casting mode for this renderer"),
                TsPropertyDef.Field("receiveShadows", "boolean")
                    .WithComment("Does this renderer receive shadows from other objects?"),
                TsPropertyDef.Field("dynamicOccludee", "boolean")
                    .WithComment("Controls if dynamic occlusion culling should be performed for this renderer"),
                TsPropertyDef.Field("staticShadowCaster", "boolean")
                    .WithComment("Should this renderer cast shadows when static?"),
                TsPropertyDef.Field("motionVectors", "MotionVectorGenerationMode")
                    .WithComment("Motion vector generation mode for this renderer"),
            });
        }
    }
}
