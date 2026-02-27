using System;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations.AssetTools
{
    [Serializable]
    public class AssetOperationOptions
    {
        public bool overwrite = false;
        public bool rename = false;
    }

    [Serializable]
    public class AssetReferenceResult
    {
        public InstanceReference<Object> reference;
    }

    public static partial class AssetTools
    {
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "Assets";

            path = path.Replace("\\", "/").Trim();

            // Remove leading slash
            if (path.StartsWith("/"))
                path = path.Substring(1);

            // Ensure starts with Assets
            if (!path.StartsWith("Assets"))
                path = "Assets/" + path;

            // Remove trailing slash
            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);

            return path;
        }

        private static void EnsureExtension(ref string path, string extension)
        {
            if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                path += extension;
        }

        private static void CreateFolderRecursive(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static string GetParentPath(string path)
        {
            return Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "";
        }

        private static string ResolveTargetPath(string targetPath, AssetOperationOptions options)
        {
            var guid = AssetDatabase.AssetPathToGUID(targetPath);
            var exists = !string.IsNullOrEmpty(guid)
                         && (File.Exists(targetPath) || AssetDatabase.IsValidFolder(targetPath));
            if (!exists) return targetPath;

            if (options is { overwrite: true })
            {
                AssetDatabase.DeleteAsset(targetPath);
                return targetPath;
            }

            if (options is { rename: true })
                return AssetDatabase.GenerateUniqueAssetPath(targetPath);

            throw new Exception($"Asset already exists at {targetPath}. Set options.overwrite or options.rename to handle.");
        }
    }
}