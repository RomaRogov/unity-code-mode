using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Canvas))]
    public class CanvasAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_RenderMode;
        private SerializedProperty m_Camera;
        private SerializedProperty m_PixelPerfect;
        private SerializedProperty m_PixelPerfectOverride;
        private SerializedProperty m_PlaneDistance;
        private SerializedProperty m_SortingLayerID;
        private SerializedProperty m_SortingOrder;
        private SerializedProperty m_TargetDisplay;
        private SerializedProperty m_OverrideSorting;
        private SerializedProperty m_ShaderChannels;
        private SerializedProperty m_UpdateRectTransformForStandalone;
        private SerializedProperty m_VertexColorAlwaysGammaSpace;

        protected override void OnEnable()
        {
            m_RenderMode = serializedObject.FindProperty("m_RenderMode");
            m_Camera = serializedObject.FindProperty("m_Camera");
            m_PixelPerfect = serializedObject.FindProperty("m_PixelPerfect");
            m_PixelPerfectOverride = serializedObject.FindProperty("m_OverridePixelPerfect");
            m_PlaneDistance = serializedObject.FindProperty("m_PlaneDistance");
            m_SortingLayerID = serializedObject.FindProperty("m_SortingLayerID");
            m_SortingOrder = serializedObject.FindProperty("m_SortingOrder");
            m_TargetDisplay = serializedObject.FindProperty("m_TargetDisplay");
            m_OverrideSorting = serializedObject.FindProperty("m_OverrideSorting");
            m_ShaderChannels = serializedObject.FindProperty("m_AdditionalShaderChannelsFlag");
            m_UpdateRectTransformForStandalone = serializedObject.FindProperty("m_UpdateRectTransformForStandalone");
            m_VertexColorAlwaysGammaSpace = serializedObject.FindProperty("m_VertexColorAlwaysGammaSpace");

            AddSettingPropertyHandler("renderMode",
                () => SerializePropertyValue(m_RenderMode),
                v => SetEnumValue(m_RenderMode, v));

            AddSettingPropertyHandler("camera",
                () => SerializeInstanceReferenceToJToken(m_Camera.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Camera, v));

            AddSettingPropertyHandler("pixelPerfect",
                () => new JValue(m_PixelPerfect.boolValue),
                v => m_PixelPerfect.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("overridePixelPerfect",
                () => new JValue(m_PixelPerfectOverride.boolValue),
                v => m_PixelPerfectOverride.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("planeDistance",
                () => new JValue(m_PlaneDistance.floatValue),
                v => m_PlaneDistance.floatValue = v.Value<float>());

            AddSettingPropertyHandler("sortingLayerID",
                () => new JValue(m_SortingLayerID.intValue),
                v => m_SortingLayerID.intValue = v.Value<int>());

            AddSettingPropertyHandler("sortingOrder",
                () => new JValue(m_SortingOrder.intValue),
                v => m_SortingOrder.intValue = v.Value<int>());

            AddSettingPropertyHandler("targetDisplay",
                () => new JValue(m_TargetDisplay.intValue),
                v => m_TargetDisplay.intValue = v.Value<int>());

            AddSettingPropertyHandler("overrideSorting",
                () => new JValue(m_OverrideSorting.boolValue),
                v => m_OverrideSorting.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("additionalShaderChannels",
                () => SerializePropertyValue(m_ShaderChannels),
                v => SetEnumValue(m_ShaderChannels, v));

            AddSettingPropertyHandler("updateRectTransformForStandalone",
                () => new JValue(m_UpdateRectTransformForStandalone.boolValue),
                v => m_UpdateRectTransformForStandalone.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("vertexColorAlwaysGammaSpace",
                () => new JValue(m_VertexColorAlwaysGammaSpace.boolValue),
                v => m_VertexColorAlwaysGammaSpace.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            var renderMode = (RenderMode)m_RenderMode.intValue;
            DumpProperty("renderMode", SerializePropertyValue(m_RenderMode));

            // Conditional properties based on render mode
            if (renderMode == RenderMode.ScreenSpaceOverlay)
            {
                DumpProperty("pixelPerfect", m_PixelPerfect.boolValue);
                DumpProperty("sortingOrder", m_SortingOrder.intValue);
                DumpProperty("targetDisplay", m_TargetDisplay.intValue);
            }
            else if (renderMode == RenderMode.ScreenSpaceCamera)
            {
                DumpProperty("pixelPerfect", m_PixelPerfect.boolValue);
                DumpProperty("camera", SerializeInstanceReferenceToJToken(m_Camera.objectReferenceValue));
                
                if (m_Camera.objectReferenceValue != null)
                {
                    DumpProperty("planeDistance", m_PlaneDistance.floatValue);
                    DumpProperty("updateRectTransformForStandalone", m_UpdateRectTransformForStandalone.boolValue);
                    DumpProperty("sortingLayerID", m_SortingLayerID.intValue);
                }
                
                DumpProperty("sortingOrder", m_SortingOrder.intValue);
            }
            else if (renderMode == RenderMode.WorldSpace)
            {
                DumpProperty("camera", SerializeInstanceReferenceToJToken(m_Camera.objectReferenceValue));
                DumpProperty("sortingLayerID", m_SortingLayerID.intValue);
                DumpProperty("sortingOrder", m_SortingOrder.intValue);
            }

            // Check if this is a nested canvas
            Canvas canvas = target as Canvas;
            if (canvas != null && canvas.transform.parent != null)
            {
                Canvas[] parentCanvas = canvas.transform.parent.GetComponentsInParent<Canvas>(true);
                if (parentCanvas != null && parentCanvas.Length > 0)
                {
                    // Nested canvas - show override properties
                    DumpProperty("overridePixelPerfect", m_PixelPerfectOverride.boolValue);
                    if (m_PixelPerfectOverride.boolValue)
                    {
                        DumpProperty("pixelPerfect", m_PixelPerfect.boolValue);
                    }
                    DumpProperty("overrideSorting", m_OverrideSorting.boolValue);
                }
            }

            DumpProperty("additionalShaderChannels", SerializePropertyValue(m_ShaderChannels));
            DumpProperty("vertexColorAlwaysGammaSpace", m_VertexColorAlwaysGammaSpace.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(RenderMode));
            GenerateEnumDefinition(typeof(AdditionalCanvasShaderChannels));

            EmitClassDefinition("Canvas", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("renderMode", "RenderMode")
                    .WithComment("Determines how the canvas is rendered (overlay, camera, or world space)"),
                
                TsPropertyDef.Reference("camera", "Camera").Nullable()
                    .WithComment("Camera used for rendering (ScreenSpaceCamera mode) or sending events (WorldSpace mode)"),
                
                TsPropertyDef.Field("pixelPerfect", "boolean")
                    .WithComment("Makes UI elements align to pixel boundaries for crisp appearance"),
                
                TsPropertyDef.Field("overridePixelPerfect", "boolean")
                    .WithComment("For nested canvas: whether to override parent's pixel perfect setting"),
                
                TsPropertyDef.Field("planeDistance", "number")
                    .WithComment("Distance from camera in ScreenSpaceCamera mode"),
                
                TsPropertyDef.Field("sortingLayerID", "number")
                    .WithComment("Sorting layer ID for rendering order"),
                
                TsPropertyDef.Field("sortingOrder", "number")
                    .WithComment("Order within the sorting layer"),
                
                TsPropertyDef.Field("targetDisplay", "number")
                    .WithComment("Display index for overlay mode"),
                
                TsPropertyDef.Field("overrideSorting", "boolean")
                    .WithComment("For nested canvas: whether to override parent's sorting"),
                
                TsPropertyDef.Field("additionalShaderChannels", "number")
                    .WithComment("Bitmask of data channels for custom shaders (TexCoord1, TexCoord2, TexCoord3, Normal, Tangent)"),
                
                TsPropertyDef.Field("updateRectTransformForStandalone", "boolean")
                    .WithComment("Whether to resize canvas for manual Camera.Render calls"),
                
                TsPropertyDef.Field("vertexColorAlwaysGammaSpace", "boolean")
                    .WithComment("Keep UI vertex colors in gamma space regardless of color space setting"),
            });
        }
    }
}



