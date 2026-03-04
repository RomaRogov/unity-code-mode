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

        // (apiName, serializedPath, enumType) — enumType is non-null for properties that Unity
        // serializes as Integer even though they represent an enum value.
        private static readonly (string api, string sp, Type enumType)[] ImportSettings =
        {
            ("textureType",              "m_TextureType",                  typeof(TextureImporterType)),
            ("textureShape",             "m_TextureShape",                 typeof(TextureImporterShape)),
            ("sRGB",                     "m_sRGBTexture",                  null),
            ("alphaSource",              "m_AlphaUsage",                   typeof(TextureImporterAlphaSource)),
            ("alphaIsTransparency",      "m_AlphaIsTransparency",          null),
            ("readable",                 "m_IsReadable",                   null),
            ("npotScale",                "m_NPOTScale",                    typeof(TextureImporterNPOTScale)),
            ("mipmaps",                  "m_EnableMipMap",                 null),
            ("mipmapFilter",             "m_MipMapMode",                   typeof(TextureImporterMipFilter)),
            ("streamingMipmaps",         "m_StreamingMipmaps",             null),
            ("streamingMipmapsPriority", "m_StreamingMipmapsPriority",     null),
            ("filterMode",               "m_TextureSettings.m_FilterMode", typeof(FilterMode)),
            ("wrapModeU",                "m_TextureSettings.m_WrapU",      typeof(TextureWrapMode)),
            ("wrapModeV",                "m_TextureSettings.m_WrapV",      typeof(TextureWrapMode)),
            ("anisoLevel",               "m_TextureSettings.m_Aniso",      null),
        };

        private static readonly (string api, string sp, Type enumType)[] SpriteSettings =
        {
            ("spriteMode",           "m_SpriteMode",                         typeof(SpriteImportMode)),
            ("pixelsPerUnit",        "m_SpritePixelsToUnits",                null),
            ("extrude",              "m_SpriteExtrude",                      null),
            ("meshType",             "m_SpriteMeshType",                     typeof(SpriteMeshType)),
            ("alignment",            "m_Alignment",                          typeof(SpriteAlignment)),
            ("pivot",                "m_SpritePivot",                        null),
            ("generatePhysicsShape", "m_SpriteGenerateFallbackPhysicsShape", null),
            ("border",               "m_SpriteBorder",                       null),
        };

        protected override void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(path)) return;

            _importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (_importer == null) return;

            foreach (var (api, sp, _) in ImportSettings)
                AddMappedHandler(api, sp);
            foreach (var (api, sp, _) in SpriteSettings)
                AddMappedHandler(api, sp);

            // Spritesheet (Multiple mode)
            // Getter: deprecated API returns SpriteMetaData[] with API-friendly field names.
            // Setter: writes via SerializedObject (m_SpriteSheet.m_Sprites) because the
            //         deprecated setter (_importer.spritesheet = ...) does not persist changes.
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

                    // Use the base serializedObject (same instance the base will Apply after the handler)
                    // so that ApplyModifiedProperties() writes these changes rather than old cached data.
                    serializedObject.Update();
                    var sprites = serializedObject.FindProperty("m_SpriteSheet.m_Sprites");
                    if (sprites == null)
                        throw new Exception("Could not find m_SpriteSheet.m_Sprites on TextureImporter.");

                    sprites.arraySize = arr.Count;
                    for (int i = 0; i < arr.Count; i++)
                    {
                        if (arr[i] is not JObject item) continue;
                        var elem = sprites.GetArrayElementAtIndex(i);

                        if (item.TryGetValue("name",      out var n)) elem.FindPropertyRelative("m_Name").stringValue    = n.Value<string>();
                        if (item.TryGetValue("rect",      out var r)) elem.FindPropertyRelative("m_Rect").rectValue      = r.DeserializeToRect();
                        if (item.TryGetValue("alignment", out var a)) elem.FindPropertyRelative("m_Alignment").intValue  = (int)ParseEnum<SpriteAlignment>(a);
                        if (item.TryGetValue("pivot",     out var p)) elem.FindPropertyRelative("m_Pivot").vector2Value  = p.DeserializeToVector2();
                        if (item.TryGetValue("border",    out var b)) elem.FindPropertyRelative("m_Border").vector4Value = b.DeserializeToVector4();
                    }
                    // No ApplyModifiedProperties/ImportAsset here — base calls ApplyModifiedProperties
                    // after all handlers, which triggers the importer reimport automatically.
                });
        }

        private void AddMappedHandler(string apiName, string serializedPath)
        {
            AddSettingPropertyHandler(apiName,
                () =>
                {
                    var so = new SerializedObject(_importer);
                    so.Update();
                    var prop = so.FindProperty(serializedPath);
                    return prop != null ? SerializePropertyValue(prop) : JValue.CreateNull();
                },
                v =>
                {
                    var so = new SerializedObject(_importer);
                    so.Update();
                    var prop = so.FindProperty(serializedPath);
                    if (prop == null)
                        throw new Exception($"Property '{serializedPath}' not found on TextureImporter.");
                    SetSerializedPropertyValue(prop, v);
                    so.ApplyModifiedProperties();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                });
        }

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

            foreach (var (api, sp, enumType) in ImportSettings)
                DumpMappedProp(so, api, sp, enumType);

            var textureTypeProp = so.FindProperty("m_TextureType");
            if (textureTypeProp == null || textureTypeProp.intValue != (int)TextureImporterType.Sprite)
                return;

            var spriteModeProp = so.FindProperty("m_SpriteMode");
            foreach (var (api, sp, enumType) in SpriteSettings)
            {
                // border is only relevant in Single and Polygon mode
                if (sp == "m_SpriteBorder" &&
                    (spriteModeProp == null ||
                     (spriteModeProp.intValue != (int)SpriteImportMode.Single &&
                      spriteModeProp.intValue != (int)SpriteImportMode.Polygon)))
                    continue;

                DumpMappedProp(so, api, sp, enumType);
            }

            if (spriteModeProp != null && spriteModeProp.intValue == (int)SpriteImportMode.Multiple)
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

        private void DumpMappedProp(SerializedObject so, string apiName, string serializedPath, Type enumType)
        {
            var prop = so.FindProperty(serializedPath);
            if (prop == null) return;

            JToken value;
            if (enumType != null && prop.propertyType == SerializedPropertyType.Integer)
                value = new JValue(Enum.GetName(enumType, prop.intValue) ?? prop.intValue.ToString());
            else
                value = SerializePropertyValue(prop);

            DumpProperty(apiName, value);
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
                TsPropertyDef.Field("width", "number").Readonly().WithHeader("Texture Info"),
                TsPropertyDef.Field("height", "number").Readonly(),
                TsPropertyDef.Field("format", "string").Readonly(),

                TsPropertyDef.Field("textureType", "TextureImporterType").WithHeader("Import Settings"),
                TsPropertyDef.Field("textureShape", "TextureImporterShape"),
                TsPropertyDef.Field("sRGB", "boolean"),
                TsPropertyDef.Field("alphaSource", "TextureImporterAlphaSource"),
                TsPropertyDef.Field("alphaIsTransparency", "boolean"),
                TsPropertyDef.Field("readable", "boolean"),
                TsPropertyDef.Field("npotScale", "TextureImporterNPOTScale"),
                TsPropertyDef.Field("mipmaps", "boolean"),
                TsPropertyDef.Field("mipmapFilter", "TextureImporterMipFilter"),
                TsPropertyDef.Field("streamingMipmaps", "boolean"),
                TsPropertyDef.Field("streamingMipmapsPriority", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("filterMode", "FilterMode"),
                TsPropertyDef.Field("wrapModeU", "TextureWrapMode"),
                TsPropertyDef.Field("wrapModeV", "TextureWrapMode"),
                TsPropertyDef.Field("anisoLevel", "number")
                    .WithDecorator("type: Integer, min: 0, max: 16"),

                TsPropertyDef.Field("spriteMode", "SpriteImportMode")
                    .WithHeader("Sprite Settings (when textureType is Sprite)").Optional(),
                TsPropertyDef.Field("pixelsPerUnit", "number").Optional(),
                TsPropertyDef.Field("extrude", "number")
                    .WithDecorator("type: Integer, min: 0, max: 32").Optional(),
                TsPropertyDef.Field("meshType", "SpriteMeshType").Optional(),
                TsPropertyDef.Field("alignment", "SpriteAlignment").Optional()
                    .WithComment("Single/Polygon mode only"),
                TsPropertyDef.Field("pivot", "Vector2").Optional()
                    .WithComment("Normalized 0-1 pivot, used when alignment is Custom."),
                TsPropertyDef.Field("generatePhysicsShape", "boolean").Optional(),
                TsPropertyDef.Field("border", "Vector4").Optional()
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
                ["name"]      = data.name,
                ["rect"]      = data.rect.SerializeToJObject(),
                ["alignment"] = ((SpriteAlignment)data.alignment).ToString(),
                ["pivot"]     = data.pivot.SerializeToJObject(),
                ["border"]    = data.border.SerializeToJObject(),
            };
        }

#endregion
    }
}