using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Image))]
    public class ImageAgentEditor : GraphicAgentEditor
    {
        private SerializedProperty m_Color;
        private SerializedProperty m_Material;
        private SerializedProperty m_RaycastTarget;
        private SerializedProperty m_RaycastPadding;
        private SerializedProperty m_Maskable;
        private SerializedProperty m_Sprite;
        private SerializedProperty m_Type;
        private SerializedProperty m_FillMethod;
        private SerializedProperty m_FillOrigin;
        private SerializedProperty m_FillAmount;
        private SerializedProperty m_FillClockwise;
        private SerializedProperty m_FillCenter;
        private SerializedProperty m_PreserveAspect;
        private SerializedProperty m_UseSpriteMesh;
        private SerializedProperty m_PixelsPerUnitMultiplier;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_Maskable = serializedObject.FindProperty("m_Maskable");
            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_FillMethod = serializedObject.FindProperty("m_FillMethod");
            m_FillOrigin = serializedObject.FindProperty("m_FillOrigin");
            m_FillAmount = serializedObject.FindProperty("m_FillAmount");
            m_FillClockwise = serializedObject.FindProperty("m_FillClockwise");
            m_FillCenter = serializedObject.FindProperty("m_FillCenter");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            m_PixelsPerUnitMultiplier = serializedObject.FindProperty("m_PixelsPerUnitMultiplier");
            
            AddSettingPropertyHandler("maskable",
                () => new JValue(m_Maskable.boolValue),
                v => m_Maskable.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("sprite",
                () => SerializeInstanceReferenceToJToken(m_Sprite.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Sprite, v));

            AddSettingPropertyHandler("type",
                () => SerializeEnumToJValue(m_Type),
                v => SetEnumValue(m_Type, v));

            AddSettingPropertyHandler("fillMethod",
                () => SerializeEnumToJValue(m_FillMethod),
                v => SetEnumValue(m_FillMethod, v));

            AddSettingPropertyHandler("fillOrigin",
                () => new JValue(m_FillOrigin.intValue),
                v => m_FillOrigin.intValue = v.Value<int>());

            AddSettingPropertyHandler("fillAmount",
                () => new JValue(m_FillAmount.floatValue),
                v => m_FillAmount.floatValue = v.Value<float>());

            AddSettingPropertyHandler("fillClockwise",
                () => new JValue(m_FillClockwise.boolValue),
                v => m_FillClockwise.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("fillCenter",
                () => new JValue(m_FillCenter.boolValue),
                v => m_FillCenter.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("preserveAspect",
                () => new JValue(m_PreserveAspect.boolValue),
                v => m_PreserveAspect.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("useSpriteMesh",
                () => new JValue(m_UseSpriteMesh.boolValue),
                v => m_UseSpriteMesh.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("pixelsPerUnitMultiplier",
                () => new JValue(m_PixelsPerUnitMultiplier.floatValue),
                v => m_PixelsPerUnitMultiplier.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            // Graphic properties
            DumpProperty("maskable", m_Maskable.boolValue);

            // Image properties
            DumpProperty("sprite", SerializeInstanceReferenceToJToken(m_Sprite.objectReferenceValue));
            DumpProperty("type", SerializeEnumToJValue(m_Type));
            DumpProperty("preserveAspect", m_PreserveAspect.boolValue);
            DumpProperty("useSpriteMesh", m_UseSpriteMesh.boolValue);
            DumpProperty("pixelsPerUnitMultiplier", m_PixelsPerUnitMultiplier.floatValue);

            // Fill properties
            DumpProperty("fillMethod", SerializeEnumToJValue(m_FillMethod));
            DumpProperty("fillOrigin", m_FillOrigin.intValue);
            DumpProperty("fillAmount", m_FillAmount.floatValue);
            DumpProperty("fillClockwise", m_FillClockwise.boolValue);
            DumpProperty("fillCenter", m_FillCenter.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            GenerateEnumDefinition(typeof(Image.Type));
            GenerateEnumDefinition(typeof(Image.FillMethod));

            EmitClassDefinition("Image", new List<TsPropertyDef>
            {
                // Graphic properties
                TsPropertyDef.Field("maskable", "boolean"),

                // Image properties
                TsPropertyDef.Reference("sprite", "Sprite").Nullable()
                    .WithHeader("Image"),
                TsPropertyDef.Field("type", "Type"),
                TsPropertyDef.Field("preserveAspect", "boolean"),
                TsPropertyDef.Field("useSpriteMesh", "boolean"),
                TsPropertyDef.Field("pixelsPerUnitMultiplier", "number")
                    .WithDecorator("type: Float, min: 0.01"),

                // Fill properties
                TsPropertyDef.Field("fillMethod", "FillMethod")
                    .WithHeader("Fill Settings"),
                TsPropertyDef.Field("fillOrigin", "number"),
                TsPropertyDef.Field("fillAmount", "number")
                    .WithDecorator("type: Float, min: 0, max: 1"),
                TsPropertyDef.Field("fillClockwise", "boolean"),
                TsPropertyDef.Field("fillCenter", "boolean"),
            }, "Graphic");
        }
    }
}
