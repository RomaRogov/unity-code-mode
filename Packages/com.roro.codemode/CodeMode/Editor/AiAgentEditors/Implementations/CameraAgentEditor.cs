using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Camera))]
    public class CameraAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_ClearFlags;
        private SerializedProperty m_BackGroundColor;
        private SerializedProperty m_CullingMask;
        private SerializedProperty m_Orthographic;
        private SerializedProperty m_OrthographicSize;
        private SerializedProperty m_FieldOfView;
        private SerializedProperty m_NearClipPlane;
        private SerializedProperty m_FarClipPlane;
        private SerializedProperty m_NormalizedViewPortRect;
        private SerializedProperty m_Depth;
        private SerializedProperty m_RenderingPath;
        private SerializedProperty m_TargetTexture;
        private SerializedProperty m_OcclusionCulling;
        private SerializedProperty m_HDR;
        private SerializedProperty m_AllowMSAA;
        private SerializedProperty m_AllowDynamicResolution;
        private SerializedProperty m_TargetDisplay;
        private SerializedProperty m_StereoTargetEye;
        private SerializedProperty m_projectionMatrixMode;
        private SerializedProperty m_FocalLength;
        private SerializedProperty m_SensorSize;
        private SerializedProperty m_LensShift;
        private SerializedProperty m_GateFitMode;

        protected override void OnEnable()
        {
            m_ClearFlags = serializedObject.FindProperty("m_ClearFlags");
            m_BackGroundColor = serializedObject.FindProperty("m_BackGroundColor");
            m_CullingMask = serializedObject.FindProperty("m_CullingMask");
            m_Orthographic = serializedObject.FindProperty("orthographic");
            m_OrthographicSize = serializedObject.FindProperty("orthographic size");
            m_FieldOfView = serializedObject.FindProperty("field of view");
            m_NearClipPlane = serializedObject.FindProperty("near clip plane");
            m_FarClipPlane = serializedObject.FindProperty("far clip plane");
            m_NormalizedViewPortRect = serializedObject.FindProperty("m_NormalizedViewPortRect");
            m_Depth = serializedObject.FindProperty("m_Depth");
            m_RenderingPath = serializedObject.FindProperty("m_RenderingPath");
            m_TargetTexture = serializedObject.FindProperty("m_TargetTexture");
            m_OcclusionCulling = serializedObject.FindProperty("m_OcclusionCulling");
            m_HDR = serializedObject.FindProperty("m_HDR");
            m_AllowMSAA = serializedObject.FindProperty("m_AllowMSAA");
            m_AllowDynamicResolution = serializedObject.FindProperty("m_AllowDynamicResolution");
            m_TargetDisplay = serializedObject.FindProperty("m_TargetDisplay");
            m_StereoTargetEye = serializedObject.FindProperty("m_StereoTargetEye");
            m_projectionMatrixMode = serializedObject.FindProperty("m_projectionMatrixMode");
            m_FocalLength = serializedObject.FindProperty("m_FocalLength");
            m_SensorSize = serializedObject.FindProperty("m_SensorSize");
            m_LensShift = serializedObject.FindProperty("m_LensShift");
            m_GateFitMode = serializedObject.FindProperty("m_GateFitMode");

            AddSettingPropertyHandler("clearFlags",
                    () => SerializePropertyValue(m_ClearFlags),
                    v => SetEnumValue(m_ClearFlags, v));

            AddSettingPropertyHandler("backgroundColor",
                () => m_BackGroundColor.colorValue.SerializeToJObject(),
                v => m_BackGroundColor.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("cullingMask",
                () => new JValue(m_CullingMask.intValue),
                v => m_CullingMask.intValue = v.Value<int>());

            AddSettingPropertyHandler("orthographic",
                () => new JValue(m_Orthographic.boolValue),
                v => m_Orthographic.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("orthographicSize",
                () => new JValue(m_OrthographicSize.floatValue),
                v => m_OrthographicSize.floatValue = v.Value<float>());

            AddSettingPropertyHandler("fieldOfView",
                () => new JValue(m_FieldOfView.floatValue),
                v => m_FieldOfView.floatValue = Mathf.Clamp(v.Value<float>(), 0.00001f, 179f));

            AddSettingPropertyHandler("nearClipPlane",
                () => new JValue(m_NearClipPlane.floatValue),
                v => m_NearClipPlane.floatValue = v.Value<float>());

            AddSettingPropertyHandler("farClipPlane",
                () => new JValue(m_FarClipPlane.floatValue),
                v => m_FarClipPlane.floatValue = v.Value<float>());

            AddSettingPropertyHandler("rect",
                () => m_NormalizedViewPortRect.rectValue.SerializeToJObject(),
                v => m_NormalizedViewPortRect.rectValue = v.DeserializeToRect());

            AddSettingPropertyHandler("depth",
                () => new JValue(m_Depth.floatValue),
                v => m_Depth.floatValue = v.Value<float>());

            AddSettingPropertyHandler("renderingPath",
                () => SerializePropertyValue(m_RenderingPath),
                v => SetEnumValue(m_RenderingPath, v));

            AddSettingPropertyHandler("targetTexture",
                () => SerializeInstanceReferenceToJToken(m_TargetTexture.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_TargetTexture, v));

            AddSettingPropertyHandler("useOcclusionCulling",
                () => new JValue(m_OcclusionCulling.boolValue),
                v => m_OcclusionCulling.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("allowHDR",
                () => new JValue(m_HDR.boolValue),
                v => m_HDR.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("allowMSAA",
                () => new JValue(m_AllowMSAA.boolValue),
                v => m_AllowMSAA.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("allowDynamicResolution",
                () => new JValue(m_AllowDynamicResolution.boolValue),
                v => m_AllowDynamicResolution.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("targetDisplay",
                () => new JValue(m_TargetDisplay.intValue),
                v => m_TargetDisplay.intValue = v.Value<int>());

            // Can be null if VR support is not enabled in the project
            if (m_StereoTargetEye != null)
            {
                AddSettingPropertyHandler("stereoTargetEye",
                    () => SerializePropertyValue(m_StereoTargetEye),
                    v => SetEnumValue(m_StereoTargetEye, v));
            }

            // Physical camera
            AddSettingPropertyHandler("usePhysicalProperties",
                () => new JValue(m_projectionMatrixMode.intValue == 2),
                v => m_projectionMatrixMode.intValue = v.Value<bool>() ? 2 : 0);

            AddSettingPropertyHandler("focalLength",
                () => new JValue(m_FocalLength.floatValue),
                v => m_FocalLength.floatValue = v.Value<float>());

            AddSettingPropertyHandler("sensorSize",
                () => m_SensorSize.vector2Value.SerializeToJObject(),
                v => m_SensorSize.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("lensShift",
                () => m_LensShift.vector2Value.SerializeToJObject(),
                v => m_LensShift.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("gateFit",
                () => SerializePropertyValue(m_GateFitMode),
                v => SetEnumValue(m_GateFitMode, v));
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("clearFlags", SerializePropertyValue(m_ClearFlags));
            DumpProperty("backgroundColor", m_BackGroundColor.colorValue.SerializeToJObject());
            DumpProperty("cullingMask", m_CullingMask.intValue);
            DumpProperty("orthographic", m_Orthographic.boolValue);

            if (m_Orthographic.boolValue)
                DumpProperty("orthographicSize", m_OrthographicSize.floatValue);
            else
                DumpProperty("fieldOfView", m_FieldOfView.floatValue);

            DumpProperty("nearClipPlane", m_NearClipPlane.floatValue);
            DumpProperty("farClipPlane", m_FarClipPlane.floatValue);
            DumpProperty("rect", m_NormalizedViewPortRect.rectValue.SerializeToJObject());
            DumpProperty("depth", m_Depth.floatValue);
            DumpProperty("renderingPath", SerializePropertyValue(m_RenderingPath));
            DumpProperty("targetTexture", SerializeInstanceReferenceToJToken(m_TargetTexture.objectReferenceValue));
            DumpProperty("useOcclusionCulling", m_OcclusionCulling.boolValue);
            DumpProperty("allowHDR", m_HDR.boolValue);
            DumpProperty("allowMSAA", m_AllowMSAA.boolValue);
            DumpProperty("allowDynamicResolution", m_AllowDynamicResolution.boolValue);
            DumpProperty("targetDisplay", m_TargetDisplay.intValue);
            if (m_StereoTargetEye != null)
            {
                DumpProperty("stereoTargetEye", SerializePropertyValue(m_StereoTargetEye));
            }

            // Physical camera
            bool isPhysical = m_projectionMatrixMode.intValue == 2;
            DumpProperty("usePhysicalProperties", isPhysical);
            if (isPhysical)
            {
                DumpProperty("focalLength", m_FocalLength.floatValue);
                DumpProperty("sensorSize", m_SensorSize.vector2Value.SerializeToJObject());
                DumpProperty("lensShift", m_LensShift.vector2Value.SerializeToJObject());
                DumpProperty("gateFit", SerializePropertyValue(m_GateFitMode));
            }
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(CameraClearFlags));
            GenerateEnumDefinition(typeof(RenderingPath));
            if (m_StereoTargetEye != null)
            {
                GenerateEnumDefinition(typeof(StereoTargetEyeMask));
            }
            GenerateEnumDefinition(typeof(Camera.GateFitMode));
            
            List<TsPropertyDef> fields = new()
            {
                TsPropertyDef.Field("clearFlags", "CameraClearFlags"),
                TsPropertyDef.Field("backgroundColor", "Color"),
                TsPropertyDef.Field("cullingMask", "number")
                    .WithComment("Bitmask selecting which layers to render"),
                TsPropertyDef.Field("orthographic", "boolean"),
                TsPropertyDef.Field("orthographicSize", "number")
                    .WithComment("Half-size of the camera in orthographic mode"),
                TsPropertyDef.Field("fieldOfView", "number")
                    .WithDecorator("type: Float, min: 0.00001, max: 179")
                    .WithComment("Vertical field of view in degrees (perspective mode)"),
                TsPropertyDef.Field("nearClipPlane", "number")
                    .WithDecorator("type: Float"),
                TsPropertyDef.Field("farClipPlane", "number")
                    .WithDecorator("type: Float"),
                TsPropertyDef.Field("rect", "Rect")
                    .WithComment("Normalized viewport rectangle (x, y, width, height) in 0-1 range"),
                TsPropertyDef.Field("depth", "number")
                    .WithDecorator("type: Float")
                    .WithComment("Rendering order; higher depth cameras render on top"),
                TsPropertyDef.Field("renderingPath", "RenderingPath"),
                TsPropertyDef.Reference("targetTexture", "RenderTexture").Nullable(),
                TsPropertyDef.Field("useOcclusionCulling", "boolean"),
                TsPropertyDef.Field("allowHDR", "boolean"),
                TsPropertyDef.Field("allowMSAA", "boolean"),
                TsPropertyDef.Field("allowDynamicResolution", "boolean"),
                TsPropertyDef.Field("targetDisplay", "number")
                    .WithDecorator("type: Integer, min: 0, max: 7"),

                // Physical camera
                TsPropertyDef.Field("usePhysicalProperties", "boolean")
                    .WithHeader("Physical Camera"),
                TsPropertyDef.Field("focalLength", "number")
                    .WithDecorator("type: Float")
                    .WithComment("Focal length in mm"),
                TsPropertyDef.Field("sensorSize", "Vector2")
                    .WithComment("Sensor size in mm"),
                TsPropertyDef.Field("lensShift", "Vector2")
                    .WithComment("Lens offset relative to sensor size"),
                TsPropertyDef.Field("gateFit", "GateFitMode"),
            };

            if (m_StereoTargetEye != null)
            {
                fields.Insert(17, TsPropertyDef.Field("stereoTargetEye", "StereoTargetEyeMask"));
            }
            
            EmitClassDefinition("Camera", fields);
        }
    }
}
