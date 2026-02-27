using System;
using System.IO;
using System.Linq;
using CodeMode.Editor.Tools.Attributes;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations.AssetTools
{
    public static partial class AssetTools
    {
        [UtcpTool("Get asset reference by path. Supports subasset paths like 'Assets/Model.fbx/MeshName'.",
            httpMethod: "GET",
            tags: new[] { "asset", "get", "path", "find", "look", "subasset" })]
        public static AssetReferenceResult AssetGetAtPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new Exception("assetPath is required");

            var path = NormalizePath(assetPath);

            // Try the full path as a direct asset first
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset != null)
                    return new AssetReferenceResult { reference = new InstanceReference<Object>(asset) };
            }

            // Try parent path as asset, last segment as subasset name
            var lastSlash = path.LastIndexOf('/');
            if (lastSlash > 0)
            {
                var parentPath = path.Substring(0, lastSlash);
                var subAssetName = path.Substring(lastSlash + 1);
                var parentGuid = AssetDatabase.AssetPathToGUID(parentPath);

                if (!string.IsNullOrEmpty(parentGuid))
                {
                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(parentPath);
                    var match = allAssets.FirstOrDefault(a =>
                        a != null && string.Equals(a.name, subAssetName, StringComparison.Ordinal));

                    if (match != null)
                        return new AssetReferenceResult { reference = new InstanceReference<Object>(match) };

                    var available = allAssets
                        .Where(a => a != null)
                        .Select(a => $"{a.name} ({a.GetType().Name})");
                    throw new Exception(
                        $"SubAsset '{subAssetName}' not found in {parentPath}. " +
                        $"Available: [{string.Join(", ", available)}]");
                }
            }

            throw new FileNotFoundException($"Asset not found at path: {path}");
        }
    }
}