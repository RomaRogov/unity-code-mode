using System;
using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Mesh))]
    public class ModelImporterAgentEditor : AiAgentEditor
    {
        private ModelImporter _importer;

        // Scene settings
        private static readonly string[] SceneSettingPaths =
        {
            "m_GlobalScale",
            "m_UseFileScale",
            "m_ImportVisibility",
            "m_ImportCameras",
            "m_ImportLights",
            "m_PreserveHierarchy",
            "m_SortHierarchyByName",
        };

        // Mesh settings
        private static readonly string[] MeshSettingPaths =
        {
            "m_MeshCompression",
            "m_IsReadable",
            "m_AddColliders",
            "m_KeepQuads",
            "m_WeldVertices",
            "m_IndexFormat",
            "m_SwapUVChannels",
            "m_GenerateSecondaryUV",
            "m_ImportBlendShapes",
        };

        // Normals & tangents
        private static readonly string[] NormalSettingPaths =
        {
            "m_NormalImportMode",
            "m_NormalCalculationMode",
            "m_NormalSmoothAngle",
            "m_TangentImportMode",
        };

        // Animation
        private static readonly string[] AnimationSettingPaths =
        {
            "m_ImportAnimation",
            "m_AnimationType",
            "m_AnimationCompression",
            "m_AnimationRotationError",
            "m_AnimationPositionError",
            "m_AnimationScaleError",
            "m_AnimationWrapMode",
        };

        // Materials
        private static readonly string[] MaterialSettingPaths =
        {
            "m_ImportMaterials",
            "m_MaterialLocation",
            "m_MaterialName",
            "m_MaterialSearch",
        };

        protected override void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(path)) return;

            _importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (_importer == null) return;

            foreach (var sp in SceneSettingPaths)
                AddSerializedHandler(sp);
            foreach (var sp in MeshSettingPaths)
                AddSerializedHandler(sp);
            foreach (var sp in NormalSettingPaths)
                AddSerializedHandler(sp);
            foreach (var sp in AnimationSettingPaths)
                AddSerializedHandler(sp);
            foreach (var sp in MaterialSettingPaths)
                AddSerializedHandler(sp);
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
                            $"Serialized property '{serializedPath}' not found on ModelImporter.");
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
                default:
                    throw new Exception(
                        $"Unsupported property type {prop.propertyType} for '{prop.propertyPath}'.");
            }
        }

        #endregion

        #region Dump

        protected override void OnDumpRequested()
        {
            var mesh = (Mesh)target;

            // Read-only mesh info
            DumpProperty("vertexCount", mesh.vertexCount);
            DumpProperty("subMeshCount", mesh.subMeshCount);
            DumpProperty("triangleCount", mesh.triangles.Length / 3);
            DumpProperty("bounds", mesh.bounds.SerializeToJObject());
            DumpProperty("isReadable", mesh.isReadable);
            DumpProperty("bindposeCount", mesh.bindposes.Length);
            DumpProperty("blendShapeCount", mesh.blendShapeCount);

            if (_importer == null) return;

            var so = new SerializedObject(_importer);
            so.Update();

            // Scene settings
            foreach (var path in SceneSettingPaths)
                DumpSerializedProp(so, path);

            // Mesh settings
            foreach (var path in MeshSettingPaths)
                DumpSerializedProp(so, path);

            // Normals & tangents
            var normalMode = so.FindProperty("m_NormalImportMode");
            if (normalMode != null)
            {
                DumpProperty("m_NormalImportMode", SerializePropertyValue(normalMode));

                // Only show calculation settings when mode is Calculate (1)
                if (normalMode.intValue == 1)
                {
                    DumpSerializedProp(so, "m_NormalCalculationMode");
                    DumpSerializedProp(so, "m_NormalSmoothAngle");
                }
            }
            DumpSerializedProp(so, "m_TangentImportMode");

            // Animation settings — only when importing animation
            var importAnim = so.FindProperty("m_ImportAnimation");
            if (importAnim != null)
            {
                DumpProperty("m_ImportAnimation", SerializePropertyValue(importAnim));
                if (importAnim.boolValue)
                {
                    foreach (var path in AnimationSettingPaths)
                    {
                        if (path == "m_ImportAnimation") continue;
                        DumpSerializedProp(so, path);
                    }
                }
            }

            // Material settings — only when importing materials
            var importMat = so.FindProperty("m_ImportMaterials");
            if (importMat != null)
            {
                DumpProperty("m_ImportMaterials", SerializePropertyValue(importMat));
                if (importMat.boolValue)
                {
                    foreach (var path in MaterialSettingPaths)
                    {
                        if (path == "m_ImportMaterials") continue;
                        DumpSerializedProp(so, path);
                    }
                }
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
            // Enums
            GenerateEnumDefinition(typeof(ModelImporterMeshCompression));
            GenerateEnumDefinition(typeof(ModelImporterIndexFormat));
            GenerateEnumDefinition(typeof(ModelImporterNormals));
            GenerateEnumDefinition(typeof(ModelImporterNormalCalculationMode));
            GenerateEnumDefinition(typeof(ModelImporterTangents));
            GenerateEnumDefinition(typeof(ModelImporterAnimationType));
            GenerateEnumDefinition(typeof(ModelImporterAnimationCompression));
            GenerateEnumDefinition(typeof(ModelImporterMaterialLocation));
            GenerateEnumDefinition(typeof(ModelImporterMaterialName));
            GenerateEnumDefinition(typeof(ModelImporterMaterialSearch));
            GenerateEnumDefinition(typeof(WrapMode));

            EmitClassDefinition("Mesh", new List<TsPropertyDef>
            {
                // Read-only mesh info
                TsPropertyDef.Field("vertexCount", "number").Readonly().WithHeader("Mesh Info"),
                TsPropertyDef.Field("subMeshCount", "number").Readonly(),
                TsPropertyDef.Field("triangleCount", "number").Readonly(),
                TsPropertyDef.Field("bounds", "Bounds").Readonly(),
                TsPropertyDef.Field("isReadable", "boolean").Readonly(),
                TsPropertyDef.Field("bindposeCount", "number").Readonly()
                    .WithComment("Number of bind poses (for skinned meshes)"),
                TsPropertyDef.Field("blendShapeCount", "number").Readonly(),

                // Scene settings
                TsPropertyDef.Field("m_GlobalScale", "number").WithHeader("Scene Settings")
                    .WithDecorator("type: Float"),
                TsPropertyDef.Field("m_UseFileScale", "boolean"),
                TsPropertyDef.Field("m_ImportVisibility", "boolean"),
                TsPropertyDef.Field("m_ImportCameras", "boolean"),
                TsPropertyDef.Field("m_ImportLights", "boolean"),
                TsPropertyDef.Field("m_PreserveHierarchy", "boolean"),
                TsPropertyDef.Field("m_SortHierarchyByName", "boolean"),

                // Mesh settings
                TsPropertyDef.Field("m_MeshCompression", "ModelImporterMeshCompression")
                    .WithHeader("Mesh Settings"),
                TsPropertyDef.Field("m_IsReadable", "boolean"),
                TsPropertyDef.Field("m_AddColliders", "boolean")
                    .WithComment("Generate mesh colliders for imported meshes"),
                TsPropertyDef.Field("m_KeepQuads", "boolean"),
                TsPropertyDef.Field("m_WeldVertices", "boolean"),
                TsPropertyDef.Field("m_IndexFormat", "ModelImporterIndexFormat"),
                TsPropertyDef.Field("m_SwapUVChannels", "boolean"),
                TsPropertyDef.Field("m_GenerateSecondaryUV", "boolean")
                    .WithComment("Generate lightmap UVs"),
                TsPropertyDef.Field("m_ImportBlendShapes", "boolean"),

                // Normals & tangents
                TsPropertyDef.Field("m_NormalImportMode", "ModelImporterNormals")
                    .WithHeader("Normals & Tangents"),
                TsPropertyDef.Field("m_NormalCalculationMode", "ModelImporterNormalCalculationMode")
                    .Optional().WithComment("Only when m_NormalImportMode is Calculate"),
                TsPropertyDef.Field("m_NormalSmoothAngle", "number").Optional()
                    .WithDecorator("type: Float, min: 0, max: 180")
                    .WithComment("Only when m_NormalImportMode is Calculate"),
                TsPropertyDef.Field("m_TangentImportMode", "ModelImporterTangents"),

                // Animation
                TsPropertyDef.Field("m_ImportAnimation", "boolean").WithHeader("Animation"),
                TsPropertyDef.Field("m_AnimationType", "ModelImporterAnimationType").Optional()
                    .WithComment("Only when m_ImportAnimation is true"),
                TsPropertyDef.Field("m_AnimationCompression", "ModelImporterAnimationCompression")
                    .Optional().WithComment("Only when m_ImportAnimation is true"),
                TsPropertyDef.Field("m_AnimationRotationError", "number").Optional()
                    .WithDecorator("type: Float"),
                TsPropertyDef.Field("m_AnimationPositionError", "number").Optional()
                    .WithDecorator("type: Float"),
                TsPropertyDef.Field("m_AnimationScaleError", "number").Optional()
                    .WithDecorator("type: Float"),
                TsPropertyDef.Field("m_AnimationWrapMode", "WrapMode").Optional(),

                // Materials
                TsPropertyDef.Field("m_ImportMaterials", "boolean").WithHeader("Materials"),
                TsPropertyDef.Field("m_MaterialLocation", "ModelImporterMaterialLocation").Optional()
                    .WithComment("Only when m_ImportMaterials is true"),
                TsPropertyDef.Field("m_MaterialName", "ModelImporterMaterialName").Optional()
                    .WithComment("Only when m_ImportMaterials is true"),
                TsPropertyDef.Field("m_MaterialSearch", "ModelImporterMaterialSearch").Optional()
                    .WithComment("Only when m_ImportMaterials is true"),
            });
        }

        #endregion
    }
}
