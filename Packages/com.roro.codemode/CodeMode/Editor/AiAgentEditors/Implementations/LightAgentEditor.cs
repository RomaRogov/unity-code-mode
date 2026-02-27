using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Light))]
    public class LightAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Type;
        private SerializedProperty m_Shape;
        private SerializedProperty m_Color;
        private SerializedProperty m_Intensity;
        private SerializedProperty m_Range;
        private SerializedProperty m_SpotAngle;
        private SerializedProperty m_InnerSpotAngle;
        private SerializedProperty m_CookieSize;
        private SerializedProperty m_Cookie;
        private SerializedProperty m_DrawHalo;
        private SerializedProperty m_Flare;
        private SerializedProperty m_RenderMode;
        private SerializedProperty m_CullingMask;
        private SerializedProperty m_RenderingLayerMask;
        private SerializedProperty m_Lightmapping;
        private SerializedProperty m_AreaSize;
        private SerializedProperty m_BounceIntensity;
        private SerializedProperty m_ColorTemperature;
        private SerializedProperty m_UseColorTemperature;
        private SerializedProperty m_Shadows;

        // Shadow sub-properties
        private SerializedProperty m_ShadowType;
        private SerializedProperty m_ShadowStrength;
        private SerializedProperty m_ShadowBias;
        private SerializedProperty m_ShadowNormalBias;
        private SerializedProperty m_ShadowNearPlane;
        private SerializedProperty m_ShadowResolution;

        protected override void OnEnable()
        {
            m_Type = serializedObject.FindProperty("m_Type");
            m_Shape = serializedObject.FindProperty("m_Shape");
            m_Color = serializedObject.FindProperty("m_Color");
            m_Intensity = serializedObject.FindProperty("m_Intensity");
            m_Range = serializedObject.FindProperty("m_Range");
            m_SpotAngle = serializedObject.FindProperty("m_SpotAngle");
            m_InnerSpotAngle = serializedObject.FindProperty("m_InnerSpotAngle");
            m_CookieSize = serializedObject.FindProperty("m_CookieSize");
            m_Cookie = serializedObject.FindProperty("m_Cookie");
            m_DrawHalo = serializedObject.FindProperty("m_DrawHalo");
            m_Flare = serializedObject.FindProperty("m_Flare");
            m_RenderMode = serializedObject.FindProperty("m_RenderMode");
            m_CullingMask = serializedObject.FindProperty("m_CullingMask");
            m_RenderingLayerMask = serializedObject.FindProperty("m_RenderingLayerMask");
            m_Lightmapping = serializedObject.FindProperty("m_Lightmapping");
            m_AreaSize = serializedObject.FindProperty("m_AreaSize");
            m_BounceIntensity = serializedObject.FindProperty("m_BounceIntensity");
            m_ColorTemperature = serializedObject.FindProperty("m_ColorTemperature");
            m_UseColorTemperature = serializedObject.FindProperty("m_UseColorTemperature");
            m_Shadows = serializedObject.FindProperty("m_Shadows");

            if (m_Shadows != null)
            {
                m_ShadowType = m_Shadows.FindPropertyRelative("m_Type");
                m_ShadowStrength = m_Shadows.FindPropertyRelative("m_Strength");
                m_ShadowBias = m_Shadows.FindPropertyRelative("m_Bias");
                m_ShadowNormalBias = m_Shadows.FindPropertyRelative("m_NormalBias");
                m_ShadowNearPlane = m_Shadows.FindPropertyRelative("m_NearPlane");
                m_ShadowResolution = m_Shadows.FindPropertyRelative("m_Resolution");
            }

            // Type
            AddSettingPropertyHandler("type",
                () => SerializeEnumToJValue(m_Type),
                v => SetEnumValue(m_Type, v));

            // Core properties
            AddSettingPropertyHandler("color",
                () => m_Color.colorValue.SerializeToJObject(),
                v => m_Color.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("intensity",
                () => new JValue(m_Intensity.floatValue),
                v => m_Intensity.floatValue = v.Value<float>());

            AddSettingPropertyHandler("bounceIntensity",
                () => new JValue(m_BounceIntensity.floatValue),
                v => m_BounceIntensity.floatValue = v.Value<float>());

            AddSettingPropertyHandler("range",
                () => new JValue(m_Range.floatValue),
                v => m_Range.floatValue = v.Value<float>());

            // Spot light properties
            AddSettingPropertyHandler("spotAngle",
                () => new JValue(m_SpotAngle.floatValue),
                v => m_SpotAngle.floatValue = Mathf.Clamp(v.Value<float>(), 1f, 179f));

            if (m_InnerSpotAngle != null)
            {
                AddSettingPropertyHandler("innerSpotAngle",
                    () => new JValue(m_InnerSpotAngle.floatValue),
                    v => m_InnerSpotAngle.floatValue = v.Value<float>());
            }

            if (m_Shape != null)
            {
                AddSettingPropertyHandler("shape",
                    () => SerializeEnumToJValue(m_Shape),
                    v => SetEnumValue(m_Shape, v));
            }

            // Area light
            if (m_AreaSize != null)
            {
                AddSettingPropertyHandler("areaSize",
                    () => m_AreaSize.vector2Value.SerializeToJObject(),
                    v => m_AreaSize.vector2Value = v.DeserializeToVector2());
            }

            // Cookie
            AddSettingPropertyHandler("cookie",
                () => SerializeInstanceReferenceToJToken(m_Cookie.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Cookie, v));

            AddSettingPropertyHandler("cookieSize",
                () => new JValue(m_CookieSize.floatValue),
                v => m_CookieSize.floatValue = v.Value<float>());

            AddSettingPropertyHandler("drawHalo",
                () => new JValue(m_DrawHalo.boolValue),
                v => m_DrawHalo.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("flare",
                () => SerializeInstanceReferenceToJToken(m_Flare.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Flare, v));

            AddSettingPropertyHandler("renderMode",
                () => SerializeEnumToJValue(m_RenderMode),
                v => SetEnumValue(m_RenderMode, v));

            AddSettingPropertyHandler("cullingMask",
                () => new JValue(m_CullingMask.intValue),
                v => m_CullingMask.intValue = v.Value<int>());

            if (m_RenderingLayerMask != null)
            {
                AddSettingPropertyHandler("renderingLayerMask",
                    () => new JValue(m_RenderingLayerMask.intValue),
                    v => m_RenderingLayerMask.intValue = v.Value<int>());
            }

            AddSettingPropertyHandler("lightmapBakeType",
                () => SerializeEnumToJValue(m_Lightmapping),
                v => SetEnumValue(m_Lightmapping, v));

            AddSettingPropertyHandler("colorTemperature",
                () => new JValue(m_ColorTemperature.floatValue),
                v => m_ColorTemperature.floatValue = v.Value<float>());

            AddSettingPropertyHandler("useColorTemperature",
                () => new JValue(m_UseColorTemperature.boolValue),
                v => m_UseColorTemperature.boolValue = v.Value<bool>());

            // Shadows composite
            if (m_Shadows != null)
            {
                AddSettingPropertyHandler("shadows",
                    () => GetShadowsDump(),
                    v => SetShadows(v));
            }
        }

        private JObject GetShadowsDump()
        {
            var obj = new JObject();
            if (m_ShadowType != null)
                obj["type"] = SerializeEnumToJValue(m_ShadowType);
            if (m_ShadowResolution != null)
                obj["resolution"] = m_ShadowResolution.enumValueIndex;
            if (m_ShadowStrength != null)
                obj["strength"] = m_ShadowStrength.floatValue;
            if (m_ShadowBias != null)
                obj["bias"] = m_ShadowBias.floatValue;
            if (m_ShadowNormalBias != null)
                obj["normalBias"] = m_ShadowNormalBias.floatValue;
            if (m_ShadowNearPlane != null)
                obj["nearPlane"] = m_ShadowNearPlane.floatValue;
            return obj;
        }

        private void SetShadows(JToken v)
        {
            if (v is not JObject obj) return;

            if (obj.TryGetValue("type", out var typeVal) && m_ShadowType != null)
                SetEnumValue(m_ShadowType, typeVal);
            if (obj.TryGetValue("resolution", out var resVal) && m_ShadowResolution != null)
                m_ShadowResolution.enumValueIndex = resVal.Value<int>();
            if (obj.TryGetValue("strength", out var strVal) && m_ShadowStrength != null)
                m_ShadowStrength.floatValue = strVal.Value<float>();
            if (obj.TryGetValue("bias", out var biasVal) && m_ShadowBias != null)
                m_ShadowBias.floatValue = biasVal.Value<float>();
            if (obj.TryGetValue("normalBias", out var nbVal) && m_ShadowNormalBias != null)
                m_ShadowNormalBias.floatValue = nbVal.Value<float>();
            if (obj.TryGetValue("nearPlane", out var npVal) && m_ShadowNearPlane != null)
                m_ShadowNearPlane.floatValue = npVal.Value<float>();
        }

        protected override void OnDumpRequested()
        {
            var lightType = (LightType)m_Type.enumValueIndex;
            DumpProperty("type", lightType.ToString());
            DumpProperty("color", m_Color.colorValue.SerializeToJObject());
            DumpProperty("intensity", m_Intensity.floatValue);
            DumpProperty("bounceIntensity", m_BounceIntensity.floatValue);

            // Type-conditional properties
            if (lightType == LightType.Point || lightType == LightType.Spot)
                DumpProperty("range", m_Range.floatValue);

            if (lightType == LightType.Spot)
            {
                DumpProperty("spotAngle", m_SpotAngle.floatValue);
                if (m_InnerSpotAngle != null)
                    DumpProperty("innerSpotAngle", m_InnerSpotAngle.floatValue);
                if (m_Shape != null)
                    DumpProperty("shape", SerializeEnumToJValue(m_Shape));
            }

            if (lightType == LightType.Rectangle || lightType == LightType.Disc)
            {
                if (m_AreaSize != null)
                    DumpProperty("areaSize", m_AreaSize.vector2Value.SerializeToJObject());
            }

            DumpProperty("cookie", SerializeInstanceReferenceToJToken(m_Cookie.objectReferenceValue));
            if (lightType == LightType.Directional)
                DumpProperty("cookieSize", m_CookieSize.floatValue);

            DumpProperty("drawHalo", m_DrawHalo.boolValue);
            DumpProperty("flare", SerializeInstanceReferenceToJToken(m_Flare.objectReferenceValue));
            DumpProperty("renderMode", SerializeEnumToJValue(m_RenderMode));
            DumpProperty("cullingMask", m_CullingMask.intValue);
            if (m_RenderingLayerMask != null)
                DumpProperty("renderingLayerMask", m_RenderingLayerMask.intValue);
            DumpProperty("lightmapBakeType", SerializeEnumToJValue(m_Lightmapping));
            DumpProperty("useColorTemperature", m_UseColorTemperature.boolValue);
            if (m_UseColorTemperature.boolValue)
                DumpProperty("colorTemperature", m_ColorTemperature.floatValue);

            if (m_Shadows != null)
                DumpProperty("shadows", GetShadowsDump());
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(LightType));
            GenerateEnumDefinition(typeof(LightRenderMode));
            GenerateEnumDefinition(typeof(LightmapBakeType));
            GenerateEnumDefinition(typeof(LightShadows));
            if (m_Shape != null)
                GenerateEnumDefinition(typeof(LightShape));

            EmitClassDefinition("LightShadowSettings", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("type", "LightShadows"),
                TsPropertyDef.Field("resolution", "number")
                    .WithComment("Shadow resolution index; -1 for quality settings default"),
                TsPropertyDef.Field("strength", "number")
                    .WithDecorator("min: 0, max: 1"),
                TsPropertyDef.Field("bias", "number"),
                TsPropertyDef.Field("normalBias", "number"),
                TsPropertyDef.Field("nearPlane", "number"),
            });

            var fields = new List<TsPropertyDef>
            {
                TsPropertyDef.Field("type", "LightType"),
                TsPropertyDef.Field("color", "Color"),
                TsPropertyDef.Field("intensity", "number"),
                TsPropertyDef.Field("bounceIntensity", "number")
                    .WithComment("Indirect light multiplier"),
                TsPropertyDef.Field("range", "number")
                    .WithComment("Point/Spot only"),
                TsPropertyDef.Field("spotAngle", "number")
                    .WithDecorator("min: 1, max: 179")
                    .WithComment("Spot only — outer cone angle in degrees"),
            };

            if (m_InnerSpotAngle != null)
                fields.Add(TsPropertyDef.Field("innerSpotAngle", "number")
                    .WithComment("Spot only — inner cone angle in degrees"));

            if (m_Shape != null)
                fields.Add(TsPropertyDef.Field("shape", "LightShape")
                    .WithComment("Spot only"));

            if (m_AreaSize != null)
                fields.Add(TsPropertyDef.Field("areaSize", "Vector2")
                    .WithComment("Area light only"));

            fields.Add(TsPropertyDef.Reference("cookie", "Texture").Nullable());
            fields.Add(TsPropertyDef.Field("cookieSize", "number")
                .WithComment("Directional light cookie size"));
            fields.Add(TsPropertyDef.Field("drawHalo", "boolean"));
            fields.Add(TsPropertyDef.Reference("flare", "Flare").Nullable());
            fields.Add(TsPropertyDef.Field("renderMode", "LightRenderMode"));
            fields.Add(TsPropertyDef.Field("cullingMask", "number")
                .WithComment("Layer mask bitmask"));

            if (m_RenderingLayerMask != null)
                fields.Add(TsPropertyDef.Field("renderingLayerMask", "number"));

            fields.Add(TsPropertyDef.Field("lightmapBakeType", "LightmapBakeType"));
            fields.Add(TsPropertyDef.Field("useColorTemperature", "boolean"));
            fields.Add(TsPropertyDef.Field("colorTemperature", "number")
                .WithComment("Kelvin temperature; requires useColorTemperature=true"));
            fields.Add(TsPropertyDef.Field("shadows", "LightShadowSettings"));

            EmitClassDefinition("Light", fields);
        }
    }
}
