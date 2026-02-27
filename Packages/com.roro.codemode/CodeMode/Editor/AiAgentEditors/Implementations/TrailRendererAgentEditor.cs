using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(TrailRenderer))]
    public class TrailRendererAgentEditor : RendererAgentEditor
    {
        private SerializedProperty m_Time;
        private SerializedProperty m_MinVertexDistance;
        private SerializedProperty m_Autodestruct;
        private SerializedProperty m_Emitting;
        private SerializedProperty m_ApplyActiveColorSpace;
        private SerializedProperty m_ColorGradient;
        private SerializedProperty m_NumCornerVertices;
        private SerializedProperty m_NumCapVertices;
        private SerializedProperty m_Alignment;
        private SerializedProperty m_TextureMode;
        private SerializedProperty m_TextureScale;
        private SerializedProperty m_ShadowBias;
        private SerializedProperty m_GenerateLightingData;
        private SerializedProperty m_MaskInteraction;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Time = serializedObject.FindProperty("m_Time");
            m_MinVertexDistance = serializedObject.FindProperty("m_MinVertexDistance");
            m_Autodestruct = serializedObject.FindProperty("m_Autodestruct");
            m_Emitting = serializedObject.FindProperty("m_Emitting");
            m_ApplyActiveColorSpace = serializedObject.FindProperty("m_ApplyActiveColorSpace");
            m_ColorGradient = serializedObject.FindProperty("m_Parameters.colorGradient");
            m_NumCornerVertices = serializedObject.FindProperty("m_Parameters.numCornerVertices");
            m_NumCapVertices = serializedObject.FindProperty("m_Parameters.numCapVertices");
            m_Alignment = serializedObject.FindProperty("m_Parameters.alignment");
            m_TextureMode = serializedObject.FindProperty("m_Parameters.textureMode");
            m_TextureScale = serializedObject.FindProperty("m_Parameters.textureScale");
            m_ShadowBias = serializedObject.FindProperty("m_Parameters.shadowBias");
            m_GenerateLightingData = serializedObject.FindProperty("m_Parameters.generateLightingData");
            m_MaskInteraction = serializedObject.FindProperty("m_MaskInteraction");

            if (m_Time != null)
            {
                AddSettingPropertyHandler("time",
                    () => new JValue(m_Time.floatValue),
                    v => m_Time.floatValue = v.Value<float>());
            }

            if (m_MinVertexDistance != null)
            {
                AddSettingPropertyHandler("minVertexDistance",
                    () => new JValue(m_MinVertexDistance.floatValue),
                    v => m_MinVertexDistance.floatValue = v.Value<float>());
            }

            if (m_Autodestruct != null)
            {
                AddSettingPropertyHandler("autodestruct",
                    () => new JValue(m_Autodestruct.boolValue),
                    v => m_Autodestruct.boolValue = v.Value<bool>());
            }

            if (m_Emitting != null)
            {
                AddSettingPropertyHandler("emitting",
                    () => new JValue(m_Emitting.boolValue),
                    v => m_Emitting.boolValue = v.Value<bool>());
            }

            if (m_ApplyActiveColorSpace != null)
            {
                AddSettingPropertyHandler("applyActiveColorSpace",
                    () => new JValue(m_ApplyActiveColorSpace.boolValue),
                    v => m_ApplyActiveColorSpace.boolValue = v.Value<bool>());
            }

            if (m_ColorGradient != null)
            {
                AddSettingPropertyHandler("colorGradient",
                    () => m_ColorGradient.gradientValue.SerializeToJObject(),
                    v => m_ColorGradient.gradientValue = v.DeserializeToGradient());
            }

            if (m_NumCornerVertices != null)
            {
                AddSettingPropertyHandler("numCornerVertices",
                    () => new JValue(m_NumCornerVertices.intValue),
                    v => m_NumCornerVertices.intValue = v.Value<int>());
            }

            if (m_NumCapVertices != null)
            {
                AddSettingPropertyHandler("numCapVertices",
                    () => new JValue(m_NumCapVertices.intValue),
                    v => m_NumCapVertices.intValue = v.Value<int>());
            }

            if (m_Alignment != null)
            {
                AddSettingPropertyHandler("alignment",
                    () => SerializePropertyValue(m_Alignment),
                    v => SetEnumValue(m_Alignment, v));
            }

            if (m_TextureMode != null)
            {
                AddSettingPropertyHandler("textureMode",
                    () => SerializePropertyValue(m_TextureMode),
                    v => SetEnumValue(m_TextureMode, v));
            }

            if (m_TextureScale != null)
            {
                AddSettingPropertyHandler("textureScale",
                    () => m_TextureScale.vector2Value.SerializeToJObject(),
                    v => m_TextureScale.vector2Value = v.DeserializeToVector2());
            }

            if (m_ShadowBias != null)
            {
                AddSettingPropertyHandler("shadowBias",
                    () => new JValue(m_ShadowBias.floatValue),
                    v => m_ShadowBias.floatValue = v.Value<float>());
            }

            if (m_GenerateLightingData != null)
            {
                AddSettingPropertyHandler("generateLightingData",
                    () => new JValue(m_GenerateLightingData.boolValue),
                    v => m_GenerateLightingData.boolValue = v.Value<bool>());
            }

            if (m_MaskInteraction != null)
            {
                AddSettingPropertyHandler("maskInteraction",
                    () => SerializePropertyValue(m_MaskInteraction),
                    v => SetEnumValue(m_MaskInteraction, v));
            }
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();

            if (m_Time != null)
                DumpProperty("time", m_Time.floatValue);

            if (m_MinVertexDistance != null)
                DumpProperty("minVertexDistance", m_MinVertexDistance.floatValue);

            if (m_Autodestruct != null)
                DumpProperty("autodestruct", m_Autodestruct.boolValue);

            if (m_Emitting != null)
                DumpProperty("emitting", m_Emitting.boolValue);

            if (m_ApplyActiveColorSpace != null)
                DumpProperty("applyActiveColorSpace", m_ApplyActiveColorSpace.boolValue);

            if (m_ColorGradient != null)
                DumpProperty("colorGradient", m_ColorGradient.gradientValue.SerializeToJObject());

            if (m_NumCornerVertices != null)
                DumpProperty("numCornerVertices", m_NumCornerVertices.intValue);

            if (m_NumCapVertices != null)
                DumpProperty("numCapVertices", m_NumCapVertices.intValue);

            if (m_Alignment != null)
                DumpProperty("alignment", SerializePropertyValue(m_Alignment));

            if (m_TextureMode != null)
                DumpProperty("textureMode", SerializePropertyValue(m_TextureMode));

            if (m_TextureScale != null)
                DumpProperty("textureScale", m_TextureScale.vector2Value.SerializeToJObject());

            if (m_ShadowBias != null)
                DumpProperty("shadowBias", m_ShadowBias.floatValue);

            if (m_GenerateLightingData != null)
                DumpProperty("generateLightingData", m_GenerateLightingData.boolValue);

            if (m_MaskInteraction != null)
                DumpProperty("maskInteraction", SerializePropertyValue(m_MaskInteraction));
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            GenerateEnumDefinition(typeof(LineAlignment));
            GenerateEnumDefinition(typeof(LineTextureMode));
            GenerateEnumDefinition(typeof(SpriteMaskInteraction));

            EmitClassDefinition("TrailRenderer", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("time", "number")
                    .WithComment("How long does the trail take to fade out (seconds)"),
                TsPropertyDef.Field("minVertexDistance", "number")
                    .WithComment("Minimum distance between anchor points of the trail"),
                TsPropertyDef.Field("autodestruct", "boolean")
                    .WithComment("Should the GameObject be destroyed when the trail has faded out?"),
                TsPropertyDef.Field("emitting", "boolean")
                    .WithComment("Is the trail currently emitting?"),
                TsPropertyDef.Field("applyActiveColorSpace", "boolean")
                    .WithComment("Apply active color space to trail gradient"),
                TsPropertyDef.Field("colorGradient", "Gradient")
                    .WithComment("Color gradient describing color of trail over its lifetime"),
                TsPropertyDef.Field("numCornerVertices", "number")
                    .WithComment("Number of vertices added for each corner (0-90)"),
                TsPropertyDef.Field("numCapVertices", "number")
                    .WithComment("Number of vertices added for each end cap (0-90)"),
                TsPropertyDef.Field("alignment", "LineAlignment")
                    .WithComment("Trail facing direction: View or TransformZ"),
                TsPropertyDef.Field("textureMode", "LineTextureMode")
                    .WithComment("How the trail texture is applied"),
                TsPropertyDef.Field("textureScale", "Vector2")
                    .WithComment("Texture tiling scale"),
                TsPropertyDef.Field("shadowBias", "number")
                    .WithComment("Shadow bias for the trail"),
                TsPropertyDef.Field("generateLightingData", "boolean")
                    .WithComment("Generate normals and tangents for lighting"),
                TsPropertyDef.Field("maskInteraction", "SpriteMaskInteraction")
                    .WithComment("How the trail interacts with SpriteMask"),
            }, "Renderer");
        }
    }
}
