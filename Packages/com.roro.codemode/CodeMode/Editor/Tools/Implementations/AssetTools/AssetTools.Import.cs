using System;
using System.IO;
using CodeMode.Editor.Tools.Attributes;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations.AssetTools
{
    public static partial class AssetTools
    {
        [Serializable]
        public class AssetImportInput : UtcpInput
        {
            [Tooltip("Source absolute filesystem path of the file to import")]
            public string sourceFilesystemPath;
            [Tooltip("Target asset path within the project (e.g. 'Assets/Textures/MyTexture.png')")]
            public string targetAssetPath;
            [Tooltip("Optional image type override for texture imports")]
            public TextureImporterType imageType = TextureImporterType.Default;
            [Tooltip("Additional options for asset import")]
            [CanBeNull] public AssetOperationOptions options;
        }

        [UtcpTool("Import an external file as an asset into the project",
            httpMethod: "POST",
            tags: new[] { "asset", "import", "file", "external" })]
        public static async UniTask<AssetReferenceResult> AssetImport(AssetImportInput input)
        {
            if (string.IsNullOrEmpty(input.sourceFilesystemPath))
                throw new Exception("sourceFilesystemPath is required");

            if (string.IsNullOrEmpty(input.targetAssetPath))
                throw new Exception("targetAssetPath is required");

            var sourcePath = input.sourceFilesystemPath;

            // Expand tilde for home directory
            if (sourcePath.StartsWith("~"))
                sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), sourcePath.Substring(1));

            sourcePath = Path.GetFullPath(sourcePath);

            if (!File.Exists(sourcePath))
                throw new Exception($"Source file not found: {sourcePath}");

            var targetPath = NormalizePath(input.targetAssetPath);
            
            // Handle overwrite/rename manually to support safe file replacement
            var guid = AssetDatabase.AssetPathToGUID(targetPath);
            var exists = !string.IsNullOrEmpty(guid) && (File.Exists(targetPath) || AssetDatabase.IsValidFolder(targetPath));

            if (exists)
            {
                if (input.options?.rename == true)
                {
                    targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
                }
                else if (input.options?.overwrite == true)
                {
                    // If target is a folder, must delete it via AssetDatabase as we can't overwrite folder with file
                    if (AssetDatabase.IsValidFolder(targetPath))
                    {
                        AssetDatabase.DeleteAsset(targetPath);
                    }
                    // For files, we will use File.Delete later to preserve metadata
                }
                else
                {
                    throw new Exception($"Asset already exists at {targetPath}. Set options.overwrite or options.rename to handle.");
                }
            }

            // Ensure parent directory exists
            var parentDir = Path.GetDirectoryName(targetPath)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(parentDir) && !AssetDatabase.IsValidFolder(parentDir))
                CreateFolderRecursive(parentDir);

            var fullTargetPath = Path.GetFullPath(targetPath);
            
            // Safe Import Logic: Copy to temp, then to target
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(sourcePath, tempPath, true);

            try 
            {
                AssetDatabase.StartAssetEditing();
                
                // If overwriting a file, delete the file but keep the .meta (by using File.Delete)
                if (File.Exists(fullTargetPath))
                    File.Delete(fullTargetPath);

                File.Copy(tempPath, fullTargetPath);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                    
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            }

            await UniTask.Yield();

            // Apply texture importer settings if applicable
            if (input.imageType != TextureImporterType.Default)
            {
                var importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = input.imageType;
                    importer.SaveAndReimport();
                    await UniTask.Yield();
                }
            }

            var asset = AssetDatabase.LoadMainAssetAtPath(targetPath);
            if (asset == null)
                throw new Exception($"Failed to load imported asset at path: {targetPath}");

            return new AssetReferenceResult
            {
                reference = new InstanceReference<Object>(asset)
            };
        }
    }
}