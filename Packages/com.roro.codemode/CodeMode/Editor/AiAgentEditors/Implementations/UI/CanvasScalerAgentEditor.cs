using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(CanvasScaler))]
    public class CanvasScalerAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_UiScaleMode;
        private SerializedProperty m_ScaleFactor;
        private SerializedProperty m_ReferenceResolution;
        private SerializedProperty m_ScreenMatchMode;
        private SerializedProperty m_MatchWidthOrHeight;
        private SerializedProperty m_PhysicalUnit;
        private SerializedProperty m_FallbackScreenDPI;
        private SerializedProperty m_DefaultSpriteDPI;
        private SerializedProperty m_DynamicPixelsPerUnit;
        private SerializedProperty m_ReferencePixelsPerUnit;

        protected override void OnEnable()
        {
            m_UiScaleMode = serializedObject.FindProperty("m_UiScaleMode");
            m_ScaleFactor = serializedObject.FindProperty("m_ScaleFactor");
            m_ReferenceResolution = serializedObject.FindProperty("m_ReferenceResolution");
            m_ScreenMatchMode = serializedObject.FindProperty("m_ScreenMatchMode");
            m_MatchWidthOrHeight = serializedObject.FindProperty("m_MatchWidthOrHeight");
            m_PhysicalUnit = serializedObject.FindProperty("m_PhysicalUnit");
            m_FallbackScreenDPI = serializedObject.FindProperty("m_FallbackScreenDPI");
            m_DefaultSpriteDPI = serializedObject.FindProperty("m_DefaultSpriteDPI");
            m_DynamicPixelsPerUnit = serializedObject.FindProperty("m_DynamicPixelsPerUnit");
            m_ReferencePixelsPerUnit = serializedObject.FindProperty("m_ReferencePixelsPerUnit");

            AddSettingPropertyHandler("uiScaleMode",
                () => SerializeEnumToJValue(m_UiScaleMode),
                v => SetEnumValue(m_UiScaleMode, v));

            AddSettingPropertyHandler("scaleFactor",
                () => new JValue(m_ScaleFactor.floatValue),
                v => m_ScaleFactor.floatValue = v.Value<float>());

            AddSettingPropertyHandler("referenceResolution",
                () => m_ReferenceResolution.vector2Value.SerializeToJObject(),
                v => m_ReferenceResolution.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("screenMatchMode",
                () => SerializeEnumToJValue(m_ScreenMatchMode),
                v => SetEnumValue(m_ScreenMatchMode, v));

            AddSettingPropertyHandler("matchWidthOrHeight",
                () => new JValue(m_MatchWidthOrHeight.floatValue),
                v => m_MatchWidthOrHeight.floatValue = v.Value<float>());

            AddSettingPropertyHandler("physicalUnit",
                () => SerializeEnumToJValue(m_PhysicalUnit),
                v => SetEnumValue(m_PhysicalUnit, v));

            AddSettingPropertyHandler("fallbackScreenDPI",
                () => new JValue(m_FallbackScreenDPI.floatValue),
                v => m_FallbackScreenDPI.floatValue = v.Value<float>());

            AddSettingPropertyHandler("defaultSpriteDPI",
                () => new JValue(m_DefaultSpriteDPI.floatValue),
                v => m_DefaultSpriteDPI.floatValue = v.Value<float>());

            AddSettingPropertyHandler("dynamicPixelsPerUnit",
                () => new JValue(m_DynamicPixelsPerUnit.floatValue),
                v => m_DynamicPixelsPerUnit.floatValue = v.Value<float>());

            AddSettingPropertyHandler("referencePixelsPerUnit",
                () => new JValue(m_ReferencePixelsPerUnit.floatValue),
                v => m_ReferencePixelsPerUnit.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("uiScaleMode", SerializeEnumToJValue(m_UiScaleMode));
            
            switch ((CanvasScaler.ScaleMode)m_UiScaleMode.enumValueIndex)
            {
                case CanvasScaler.ScaleMode.ConstantPixelSize:
                    DumpProperty("scaleFactor", m_ScaleFactor.floatValue);
                    break;
                case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                    DumpProperty("referenceResolution", m_ReferenceResolution.vector2Value.SerializeToJObject());
                    DumpProperty("screenMatchMode", SerializeEnumToJValue(m_ScreenMatchMode));
                    DumpProperty("matchWidthOrHeight", m_MatchWidthOrHeight.floatValue);
                    break;
                case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                    DumpProperty("physicalUnit", SerializeEnumToJValue(m_PhysicalUnit));
                    DumpProperty("fallbackScreenDPI", m_FallbackScreenDPI.floatValue);
                    DumpProperty("defaultSpriteDPI", m_DefaultSpriteDPI.floatValue);
                    break;
            }

            DumpProperty("dynamicPixelsPerUnit", m_DynamicPixelsPerUnit.floatValue);
            DumpProperty("referencePixelsPerUnit", m_ReferencePixelsPerUnit.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(CanvasScaler.ScaleMode));
            GenerateEnumDefinition(typeof(CanvasScaler.ScreenMatchMode));
            GenerateEnumDefinition(typeof(CanvasScaler.Unit));

            EmitClassDefinition("CanvasScaler", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("uiScaleMode", "ScaleMode"),
                TsPropertyDef.Field("scaleFactor", "number"),
                TsPropertyDef.Field("referenceResolution", "Vector2"),
                TsPropertyDef.Field("screenMatchMode", "ScreenMatchMode"),
                TsPropertyDef.Field("matchWidthOrHeight", "number"),
                TsPropertyDef.Field("physicalUnit", "Unit"),
                TsPropertyDef.Field("fallbackScreenDPI", "number"),
                TsPropertyDef.Field("defaultSpriteDPI", "number"),
                TsPropertyDef.Field("dynamicPixelsPerUnit", "number"),
                TsPropertyDef.Field("referencePixelsPerUnit", "number"),
            });
        }
    }
}

