using System.Collections.Generic;
using CodeMode.Editor.AiAgentEditors;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace CodeMode.Editor.CustomSettingsEditors.Implementations
{
    [CustomSettingsEditor("Lighting")]
    public class LightingSettingsEditor : AiSettingsEditor
    {
        protected override void OnEnable()
        {
            AddSettingPropertyHandler("ambientMode",
                () => new JValue(RenderSettings.ambientMode.ToString()),
                v => RenderSettings.ambientMode = ParseEnum<AmbientMode>(v));
            AddSettingPropertyHandler("ambientSkyColor",
                () => RenderSettings.ambientSkyColor.SerializeToJObject(),
                v => RenderSettings.ambientSkyColor = v.DeserializeToColor());
            AddSettingPropertyHandler("ambientEquatorColor",
                () => RenderSettings.ambientEquatorColor.SerializeToJObject(),
                v => RenderSettings.ambientEquatorColor = v.DeserializeToColor());
            AddSettingPropertyHandler("ambientGroundColor",
                () => RenderSettings.ambientGroundColor.SerializeToJObject(),
                v => RenderSettings.ambientGroundColor = v.DeserializeToColor());
            AddSettingPropertyHandler("ambientLight",
                () => RenderSettings.ambientLight.SerializeToJObject(),
                v => RenderSettings.ambientLight = v.DeserializeToColor());
            AddSettingPropertyHandler("ambientIntensity",
                () => new JValue(RenderSettings.ambientIntensity),
                v => RenderSettings.ambientIntensity = v.Value<float>());
            AddSettingPropertyHandler("subtractiveShadowColor",
                () => RenderSettings.subtractiveShadowColor.SerializeToJObject(),
                v => RenderSettings.subtractiveShadowColor = v.DeserializeToColor());
            AddSettingPropertyHandler("defaultReflectionMode",
                () => new JValue(RenderSettings.defaultReflectionMode.ToString()),
                v => RenderSettings.defaultReflectionMode = ParseEnum<DefaultReflectionMode>(v));
            AddSettingPropertyHandler("defaultReflectionResolution",
                () => new JValue(RenderSettings.defaultReflectionResolution),
                v => RenderSettings.defaultReflectionResolution = v.Value<int>());
            AddSettingPropertyHandler("reflectionBounces",
                () => new JValue(RenderSettings.reflectionBounces),
                v => RenderSettings.reflectionBounces = v.Value<int>());
            AddSettingPropertyHandler("reflectionIntensity",
                () => new JValue(RenderSettings.reflectionIntensity),
                v => RenderSettings.reflectionIntensity = v.Value<float>());
            AddSettingPropertyHandler("fog",
                () => new JValue(RenderSettings.fog),
                v => RenderSettings.fog = v.Value<bool>());
            AddSettingPropertyHandler("fogMode",
                () => new JValue(RenderSettings.fogMode.ToString()),
                v => RenderSettings.fogMode = ParseEnum<FogMode>(v));
            AddSettingPropertyHandler("fogColor",
                () => RenderSettings.fogColor.SerializeToJObject(),
                v => RenderSettings.fogColor = v.DeserializeToColor());
            AddSettingPropertyHandler("fogDensity",
                () => new JValue(RenderSettings.fogDensity),
                v => RenderSettings.fogDensity = v.Value<float>());
            AddSettingPropertyHandler("fogStartDistance",
                () => new JValue(RenderSettings.fogStartDistance),
                v => RenderSettings.fogStartDistance = v.Value<float>());
            AddSettingPropertyHandler("fogEndDistance",
                () => new JValue(RenderSettings.fogEndDistance),
                v => RenderSettings.fogEndDistance = v.Value<float>());
            AddSettingPropertyHandler("flareFadeSpeed",
                () => new JValue(RenderSettings.flareFadeSpeed),
                v => RenderSettings.flareFadeSpeed = v.Value<float>());
            AddSettingPropertyHandler("flareStrength",
                () => new JValue(RenderSettings.flareStrength),
                v => RenderSettings.flareStrength = v.Value<float>());
            AddSettingPropertyHandler("haloStrength",
                () => new JValue(RenderSettings.haloStrength),
                v => RenderSettings.haloStrength = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            // Environment
            DumpProperty("skybox", SerializeInstanceReferenceToJToken(RenderSettings.skybox));
            DumpProperty("sun", SerializeInstanceReferenceToJToken(RenderSettings.sun));
            DumpProperty("ambientMode", RenderSettings.ambientMode.ToString());
            DumpProperty("ambientSkyColor", RenderSettings.ambientSkyColor.SerializeToJObject());
            DumpProperty("ambientEquatorColor", RenderSettings.ambientEquatorColor.SerializeToJObject());
            DumpProperty("ambientGroundColor", RenderSettings.ambientGroundColor.SerializeToJObject());
            DumpProperty("ambientLight", RenderSettings.ambientLight.SerializeToJObject());
            DumpProperty("ambientIntensity", RenderSettings.ambientIntensity);
            DumpProperty("subtractiveShadowColor", RenderSettings.subtractiveShadowColor.SerializeToJObject());

            // Reflections
            DumpProperty("defaultReflectionMode", RenderSettings.defaultReflectionMode.ToString());
            DumpProperty("defaultReflectionResolution", RenderSettings.defaultReflectionResolution);
            DumpProperty("reflectionBounces", RenderSettings.reflectionBounces);
            DumpProperty("reflectionIntensity", RenderSettings.reflectionIntensity);
            DumpProperty("customReflectionTexture", SerializeInstanceReferenceToJToken(RenderSettings.customReflectionTexture));

            // Fog
            DumpProperty("fog", RenderSettings.fog);
            DumpProperty("fogMode", RenderSettings.fogMode.ToString());
            DumpProperty("fogColor", RenderSettings.fogColor.SerializeToJObject());
            DumpProperty("fogDensity", RenderSettings.fogDensity);
            DumpProperty("fogStartDistance", RenderSettings.fogStartDistance);
            DumpProperty("fogEndDistance", RenderSettings.fogEndDistance);

            // Other
            DumpProperty("flareFadeSpeed", RenderSettings.flareFadeSpeed);
            DumpProperty("flareStrength", RenderSettings.flareStrength);
            DumpProperty("haloStrength", RenderSettings.haloStrength);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("LightingSettings", new List<TsPropertyDef>
            {
                TsPropertyDef.InstanceRef("skybox", "Material").WithHeader("Environment"),
                TsPropertyDef.InstanceRef("sun", "Light"),
                TsPropertyDef.Field("ambientMode", "'Skybox' | 'Trilight' | 'Flat' | 'Custom'")
                    .WithComment("Skybox, Trilight, Flat, Custom"),
                TsPropertyDef.Field("ambientSkyColor", "Color"),
                TsPropertyDef.Field("ambientEquatorColor", "Color"),
                TsPropertyDef.Field("ambientGroundColor", "Color"),
                TsPropertyDef.Field("ambientLight", "Color")
                    .WithComment("Used when ambientMode is Flat"),
                TsPropertyDef.Field("ambientIntensity", "number"),
                TsPropertyDef.Field("subtractiveShadowColor", "Color"),
                TsPropertyDef.Field("defaultReflectionMode", "'Skybox' | 'Custom'")
                    .WithComment("Skybox, Custom").WithHeader("Reflections"),
                TsPropertyDef.Field("defaultReflectionResolution", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("reflectionBounces", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("reflectionIntensity", "number"),
                TsPropertyDef.InstanceRef("customReflectionTexture", "Texture"),
                TsPropertyDef.Field("fog", "boolean").WithHeader("Fog"),
                TsPropertyDef.Field("fogMode", "'Linear' | 'Exponential' | 'ExponentialSquared'")
                    .WithComment("Linear, Exponential, ExponentialSquared"),
                TsPropertyDef.Field("fogColor", "Color"),
                TsPropertyDef.Field("fogDensity", "number"),
                TsPropertyDef.Field("fogStartDistance", "number"),
                TsPropertyDef.Field("fogEndDistance", "number"),
                TsPropertyDef.Field("flareFadeSpeed", "number").WithHeader("Other"),
                TsPropertyDef.Field("flareStrength", "number"),
                TsPropertyDef.Field("haloStrength", "number"),
            });
        }

        private static T ParseEnum<T>(JToken value) where T : struct
        {
            if (value.Type == JTokenType.String)
            {
                if (System.Enum.TryParse<T>(value.Value<string>(), out var result))
                    return result;
                throw new System.Exception($"Invalid enum value '{value}' for {typeof(T).Name}.");
            }
            return (T)(object)value.Value<int>();
        }
    }
}
