using System;
using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Texture2D))]
    public class TextureImporterAgentEditor : AiAgentEditor
    {
        private TextureImporter _importer;

        // Serialized property paths matching TextureImporterInspector.CacheSerializedProperties
        private static readonly string[] ImportSettingPaths =
        {
            "m_TextureType",
            "m_TextureShape",
            "m_sRGBTexture",
            "m_AlphaUsage",
            "m_AlphaIsTransparency",
            "m_IsReadable",
            "m_NPOTScale",
            "m_EnableMipMap",
            "m_MipMapMode",
            "m_StreamingMipmaps",
            "m_StreamingMipmapsPriority",
            "m_TextureSettings.m_FilterMode",
            "m_TextureSettings.m_WrapU",
            "m_TextureSettings.m_WrapV",
            "m_TextureSettings.m_Aniso",
        };

        private static readonly string[] SpriteSettingPaths =
        {
            "m_SpriteMode",
            "m_SpritePixelsToUnits",
            "m_SpriteExtrude",
            "m_SpriteMeshType",
            "m_Alignment",
            "m_SpritePivot",
            "m_SpriteGenerateFallbackPhysicsShape",
        };

        protected override void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(path)) return;

            _importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (_importer == null) return;

            // Register handlers for all serialized properties — each uses SO + reimport
            foreach (var sp in ImportSettingPaths)
                AddSerializedHandler(sp);
            foreach (var sp in SpriteSettingPaths)
                AddSerializedHandler(sp);

            // Spritesheet (Multiple mode) — uses deprecated API, no serialized property equivalent
            AddSettingPropertyHandler("spritesheet",
                () =>
                {
#pragma warning disable 0618
                    var sheet = _importer.spritesheet;
#pragma warning restore 0618
                    var arr = new JArray();
                    foreach (var sprite in sheet)
                        arr.Add(SerializeSpriteMetaData(sprite));
                    return arr;
                },
                v =>
                {
                    if (v is not JArray arr)
                        throw new Exception("spritesheet must be a JSON array of SpriteMetaData objects.");

                    var metaData = new SpriteMetaData[arr.Count];
                    for (int i = 0; i < arr.Count; i++)
                        metaData[i] = DeserializeSpriteMetaData((JObject)arr[i]);

#pragma warning disable 0618
                    _importer.spritesheet = metaData;
#pragma warning restore 0618
                    _importer.SaveAndReimport();
                });

            // Sprite border — stored as serialized property but managed by Sprite Editor, not the importer inspector
            AddSerializedHandler("m_SpriteBorder");
        }

        #region Serialized Property Handlers

        private void AddSerializedHandler(string serializedPath)
        {
            AddSettingPropertyHandler(serializedPath,
                () =>
                {
                    var so = new SerializedObject(_importer);
                    so.Update();
                    var prop = so.FindProperty(serializedPath);
                    if (prop == null) return JValue.CreateNull();
                    return SerializePropertyValue(prop);
                },
                v =>
                {
                    var so = new SerializedObject(_importer);
                    so.Update();
                    var prop = so.FindProperty(serializedPath);
                    if (prop == null)
                        throw new Exception(
                            $"Serialized property '{serializedPath}' not found on TextureImporter.");
                    SetPropertyValue(prop, v);
                    so.ApplyModifiedProperties();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                });
        }

        private static void SetPropertyValue(SerializedProperty prop, JToken value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = value.Value<int>();
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = value.Value<bool>();
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = value.Value<float>();
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = value.Value<string>();
                    break;
                case SerializedPropertyType.Enum:
                    if (value.Type == JTokenType.String)
                    {
                        var idx = Array.IndexOf(prop.enumNames, value.Value<string>());
                        if (idx >= 0)
                        {
                            prop.enumValueIndex = idx;
                            return;
                        }
                    }

                    prop.intValue = value.Value<int>();
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = value.DeserializeToVector2();
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = value.DeserializeToVector4();
                    break;
                default:
                    throw new Exception(
                        $"Unsupported property type {prop.propertyType} for '{prop.propertyPath}'.");
            }
        }

        #endregion

        #region Dump

        protected override void OnDumpRequested()
        {
            var tex = (Texture2D)target;
            DumpProperty("width", tex.width);
            DumpProperty("height", tex.height);
            DumpProperty("format", tex.format.ToString());

            if (_importer == null) return;

            var so = new SerializedObject(_importer);
            so.Update();

            // Import settings — matching TextureImporterInspector
            foreach (var path in ImportSettingPaths)
                DumpSerializedProp(so, path);

            // Sprite settings — only when texture type is Sprite
            var textureType = so.FindProperty("m_TextureType");
            if (textureType == null || textureType.intValue != (int)TextureImporterType.Sprite)
                return;

            foreach (var path in SpriteSettingPaths)
                DumpSerializedProp(so, path);

            // Sprite border (Single/Polygon)
            var spriteMode = so.FindProperty("m_SpriteMode");
            if (spriteMode != null && (spriteMode.intValue == (int)SpriteImportMode.Single ||
                                       spriteMode.intValue == (int)SpriteImportMode.Polygon))
            {
                DumpSerializedProp(so, "m_SpriteBorder");
            }

            // Spritesheet array (Multiple mode) — via deprecated API
            if (spriteMode != null && spriteMode.intValue == (int)SpriteImportMode.Multiple)
            {
#pragma warning disable 0618
                var sheet = _importer.spritesheet;
#pragma warning restore 0618
                var arr = new JArray();
                foreach (var sprite in sheet)
                    arr.Add(SerializeSpriteMetaData(sprite));
                DumpProperty("spritesheet", arr);
            }
        }

        private void DumpSerializedProp(SerializedObject so, string path)
        {
            var prop = so.FindProperty(path);
            if (prop != null)
                DumpProperty(path, SerializePropertyValue(prop));
        }

        #endregion

        #region Definition

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(TextureImporterType));
            GenerateEnumDefinition(typeof(TextureImporterShape));
            GenerateEnumDefinition(typeof(TextureImporterAlphaSource));
            GenerateEnumDefinition(typeof(TextureImporterNPOTScale));
            GenerateEnumDefinition(typeof(FilterMode));
            GenerateEnumDefinition(typeof(TextureWrapMode));
            GenerateEnumDefinition(typeof(SpriteImportMode));
            GenerateEnumDefinition(typeof(SpriteMeshType));
            GenerateEnumDefinition(typeof(SpriteAlignment));
            GenerateEnumDefinition(typeof(TextureImporterMipFilter));

            EmitClassDefinition("SpriteMetaData", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("name", "string"),
                TsPropertyDef.Field("rect", "Rect"),
                TsPropertyDef.Field("alignment", "SpriteAlignment"),
                TsPropertyDef.Field("pivot", "Vector2")
                    .WithComment("Used when alignment is Custom. Normalized 0-1 coordinates."),
                TsPropertyDef.Field("border", "Vector4")
                    .WithComment("9-slice border sizes: { x: left, y: bottom, z: right, w: top }"),
            });

            EmitClassDefinition("Texture2D", new List<TsPropertyDef>
            {
                // Read-only texture info
                TsPropertyDef.Field("width", "number").Readonly().WithHeader("Texture Info"),
                TsPropertyDef.Field("height", "number").Readonly(),
                TsPropertyDef.Field("format", "string").Readonly(),

                // Import settings (serialized property paths)
                TsPropertyDef.Field("m_TextureType", "TextureImporterType").WithHeader("Import Settings"),
                TsPropertyDef.Field("m_TextureShape", "TextureImporterShape"),
                TsPropertyDef.Field("m_sRGBTexture", "boolean"),
                TsPropertyDef.Field("m_AlphaUsage", "TextureImporterAlphaSource"),
                TsPropertyDef.Field("m_AlphaIsTransparency", "boolean"),
                TsPropertyDef.Field("m_IsReadable", "boolean"),
                TsPropertyDef.Field("m_NPOTScale", "TextureImporterNPOTScale"),
                TsPropertyDef.Field("m_EnableMipMap", "boolean"),
                TsPropertyDef.Field("m_MipMapMode", "TextureImporterMipFilter"),
                TsPropertyDef.Field("m_StreamingMipmaps", "boolean"),
                TsPropertyDef.Field("m_StreamingMipmapsPriority", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("m_TextureSettings.m_FilterMode", "FilterMode"),
                TsPropertyDef.Field("m_TextureSettings.m_WrapU", "TextureWrapMode"),
                TsPropertyDef.Field("m_TextureSettings.m_WrapV", "TextureWrapMode"),
                TsPropertyDef.Field("m_TextureSettings.m_Aniso", "number")
                    .WithDecorator("type: Integer, min: 0, max: 16"),

                // Sprite settings (when m_TextureType is Sprite)
                TsPropertyDef.Field("m_SpriteMode", "SpriteImportMode")
                    .WithHeader("Sprite Settings (when m_TextureType is Sprite)").Optional(),
                TsPropertyDef.Field("m_SpritePixelsToUnits", "number").Optional(),
                TsPropertyDef.Field("m_SpriteExtrude", "number")
                    .WithDecorator("type: Integer, min: 0, max: 32").Optional(),
                TsPropertyDef.Field("m_SpriteMeshType", "SpriteMeshType").Optional(),
                TsPropertyDef.Field("m_Alignment", "SpriteAlignment").Optional()
                    .WithComment("Single/Polygon mode only"),
                TsPropertyDef.Field("m_SpritePivot", "Vector2").Optional()
                    .WithComment("Normalized 0-1 pivot, used when m_Alignment is Custom."),
                TsPropertyDef.Field("m_SpriteGenerateFallbackPhysicsShape", "boolean").Optional(),
                TsPropertyDef.Field("m_SpriteBorder", "Vector4").Optional()
                    .WithComment("9-slice border: { x: left, y: bottom, z: right, w: top }. Single/Polygon mode."),
                TsPropertyDef.ArrayOf("spritesheet", "SpriteMetaData").Optional()
                    .WithComment("Per-sprite definitions. Multiple mode only. Set entire array to modify."),
            });
        }

        #endregion

        #region SpriteMetaData Helpers

        private static JObject SerializeSpriteMetaData(SpriteMetaData data)
        {
            return new JObject
            {
                ["name"] = data.name,
                ["rect"] = data.rect.SerializeToJObject(),
                ["alignment"] = ((SpriteAlignment)data.alignment).ToString(),
                ["pivot"] = data.pivot.SerializeToJObject(),
                ["border"] = data.border.SerializeToJObject(),
            };
        }

        private static SpriteMetaData DeserializeSpriteMetaData(JObject obj)
        {
            var data = new SpriteMetaData();

            if (obj.TryGetValue("name", out var n))
                data.name = n.Value<string>();
            if (obj.TryGetValue("rect", out var r))
                data.rect = r.DeserializeToRect();
            if (obj.TryGetValue("alignment", out var a))
                data.alignment = (int)ParseEnum<SpriteAlignment>(a);
            if (obj.TryGetValue("pivot", out var p))
                data.pivot = p.DeserializeToVector2();
            if (obj.TryGetValue("border", out var b))
                data.border = b.DeserializeToVector4();

            return data;
        }

        #endregion
    }
}
