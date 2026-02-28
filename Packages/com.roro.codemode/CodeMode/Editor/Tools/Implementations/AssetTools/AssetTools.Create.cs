using System;
using System.IO;
using System.Threading.Tasks;
using CodeMode.Editor.Tools.Attributes;
using CodeMode.Editor.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations.AssetTools
{
    public static partial class AssetTools
    {
        public enum AssetPreset
        {
            Folder,
            Prefab,
            Material,
            ScriptableObject,
            Scene,
            PhysicsMaterial,
            PhysicsMaterial2D,
            SpriteAtlas,
            RenderTexture,
            AnimatorController,
            AnimationClip
        }

        [Serializable]
        public class AssetCreateInput : UtcpInput
        {
            public string assetPath;

            [Tooltip("Preset type for the new asset")]
            public AssetPreset preset;

            [Tooltip("For ScriptableObjects, specify the type name")]
            [CanBeNull] public string scriptableObjectType;

            [Tooltip("Additional options for asset creation")]
            [CanBeNull] public AssetOperationOptions options;
        }

        [UtcpTool("Create empty asset or folder of given type. Returns reference to the new asset.",
            httpMethod: "POST",
            tags: new[] { "asset", "create", "new", "folder", "material" })]
        public static async Task<AssetReferenceResult> AssetCreate(AssetCreateInput input)
        {
            if (string.IsNullOrEmpty(input.assetPath))
                throw new Exception("assetPath is required");

            var path = NormalizePath(input.assetPath);

            // Resolve extension based on preset
            switch (input.preset)
            {
                case AssetPreset.Folder: break;
                case AssetPreset.Prefab: EnsureExtension(ref path, ".prefab"); break;
                case AssetPreset.Material: EnsureExtension(ref path, ".mat"); break;
                case AssetPreset.Scene: EnsureExtension(ref path, ".unity"); break;
                case AssetPreset.PhysicsMaterial: EnsureExtension(ref path, ".physicMaterial"); break;
                case AssetPreset.PhysicsMaterial2D: EnsureExtension(ref path, ".physicsMaterial2D"); break;
                case AssetPreset.SpriteAtlas: EnsureExtension(ref path, ".spriteatlas"); break;
                case AssetPreset.RenderTexture: EnsureExtension(ref path, ".renderTexture"); break;
                case AssetPreset.ScriptableObject: EnsureExtension(ref path, ".asset"); break;
                case AssetPreset.AnimatorController: EnsureExtension(ref path, ".controller"); break;
                case AssetPreset.AnimationClip: EnsureExtension(ref path, ".anim"); break;
            }

            // Handle overwrite/rename
            path = ResolveTargetPath(path, input.options);

            // Ensure parent directory exists
            var parentDir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parentDir) && !AssetDatabase.IsValidFolder(parentDir))
                CreateFolderRecursive(parentDir);

            switch (input.preset)
            {
                case AssetPreset.Folder:
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        var folderName = Path.GetFileName(path);
                        var folderParent = Path.GetDirectoryName(path);
                        AssetDatabase.CreateFolder(folderParent, folderName);
                    }
                    break;

                case AssetPreset.Prefab:
                    var tempGo = new GameObject(Path.GetFileNameWithoutExtension(path));
                    PrefabUtility.SaveAsPrefabAsset(tempGo, path);
                    Object.DestroyImmediate(tempGo);
                    break;

                case AssetPreset.Material:
                    AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")), path);
                    break;

                case AssetPreset.Scene:
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                        UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                        UnityEditor.SceneManagement.NewSceneMode.Additive);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                    break;

                case AssetPreset.PhysicsMaterial:
                    AssetDatabase.CreateAsset(new PhysicMaterial(), path);
                    break;

                case AssetPreset.PhysicsMaterial2D:
                    AssetDatabase.CreateAsset(new PhysicsMaterial2D(), path);
                    break;

                case AssetPreset.SpriteAtlas:
                    AssetDatabase.CreateAsset(new UnityEngine.U2D.SpriteAtlas(), path);
                    break;

                case AssetPreset.RenderTexture:
                    AssetDatabase.CreateAsset(new RenderTexture(256, 256, 24), path);
                    break;

                case AssetPreset.ScriptableObject:
                    if (string.IsNullOrEmpty(input.scriptableObjectType))
                        throw new Exception("scriptableObjectType is required for ScriptableObject preset");

                    var type = Type.GetType(input.scriptableObjectType);
                    if (type == null)
                    {
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            type = asm.GetType(input.scriptableObjectType);
                            if (type != null) break;
                        }
                    }

                    if (type == null || !typeof(ScriptableObject).IsAssignableFrom(type))
                        throw new Exception($"Invalid scriptableObjectType: {input.scriptableObjectType}");

                    var instance = ScriptableObject.CreateInstance(type);
                    AssetDatabase.CreateAsset(instance, path);
                    break;

                case AssetPreset.AnimatorController:
                    AnimatorController.CreateAnimatorControllerAtPath(path);
                    break;

                case AssetPreset.AnimationClip:
                    AssetDatabase.CreateAsset(new AnimationClip(), path);
                    break;

                default:
                    throw new Exception($"Unknown preset type: {input.preset}");
            }

            AssetDatabase.Refresh();
            await EditorAsync.Yield();

            var resultAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (resultAsset == null)
                throw new Exception($"Failed to load created asset at path: {path}");

            return new AssetReferenceResult
            {
                reference = new InstanceReference<Object>(resultAsset)
            };
        }
    }
}