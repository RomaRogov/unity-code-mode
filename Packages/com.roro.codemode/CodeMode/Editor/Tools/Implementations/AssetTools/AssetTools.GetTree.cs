using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public class AssetGetTreeInput : UtcpInput
        {
            [Tooltip("Asset reference to start from (overrides assetPath if provided)")]
            public InstanceReference<Object> reference;

            [Tooltip("Root path to start from")]
            [CanBeNull] public string assetPath;
        }

        [UtcpTool("Get the asset and subAsset hierarchy tree. Children have recursive structure.",
            httpMethod: "GET",
            tags: new[] { "asset", "file", "tree", "hierarchy", "folder" })]
        public static async UniTask<AssetTreeItem> AssetGetTree(AssetGetTreeInput input)
        {
            var rootPath = string.IsNullOrEmpty(input.assetPath) ? "Assets" : NormalizePath(input.assetPath);

            if (input.reference != null)
            {
                rootPath = AssetDatabase.GetAssetPath(input.reference.Instance);
            }

            if (!AssetDatabase.IsValidFolder(rootPath) && !File.Exists(rootPath))
            {
                throw new FileNotFoundException($"No asset found at {rootPath}");
            }

            return await BuildAssetTree(rootPath);
        }

        private static async UniTask<AssetTreeItem> BuildAssetTree(string rootPath)
        {
            var isFolder = AssetDatabase.IsValidFolder(rootPath);

            // Single file — return it with its subassets directly
            if (!isFolder)
                return BuildFileNode(rootPath, isRoot: true);

            // Use FindAssets to get a flat list of all assets under the root
            var guids = AssetDatabase.FindAssets("", new[] { rootPath });
            var assetPaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !string.IsNullOrEmpty(p) && p != rootPath)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            // Create root node
            var rootObj = AssetDatabase.LoadAssetAtPath<Object>(rootPath);
            var rootName = Path.GetFileName(rootPath);
            var root = new AssetTreeItem
            {
                filesystemPath = Path.Combine(Application.dataPath, "..", rootPath),
                name = string.IsNullOrEmpty(rootName) ? rootPath : rootName,
                reference = rootObj != null ? new InstanceReference<Object>(rootObj) : null,
                children = new List<AssetTreeItem>()
            };

            var nodeMap = new Dictionary<string, AssetTreeItem> { { rootPath, root } };
            int count = 0;

            foreach (var assetPath in assetPaths)
            {
                if (nodeMap.ContainsKey(assetPath)) continue;

                // Yield periodically to avoid blocking the editor
                if (++count % 50 == 0)
                    await UniTask.Yield();

                // Ensure all ancestor folders exist in the tree
                var parentPath = GetParentPath(assetPath);
                EnsureAncestorNodes(parentPath, rootPath, nodeMap);

                AssetTreeItem item;
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var folderObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    item = new AssetTreeItem
                    {
                        name = Path.GetFileName(assetPath),
                        reference = folderObj != null ? new InstanceReference<Object>(folderObj) : null,
                        children = new List<AssetTreeItem>()
                    };
                }
                else
                {
                    item = BuildFileNode(assetPath, isRoot: false);
                }

                nodeMap[assetPath] = item;

                if (nodeMap.TryGetValue(parentPath, out var parent))
                    parent.children.Add(item);
            }

            return root;
        }

        private static AssetTreeItem BuildFileNode(string assetPath, bool isRoot)
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);

            var item = new AssetTreeItem
            {
                filesystemPath = isRoot ? Path.Combine(Application.dataPath, "..", assetPath) : null,
                name = Path.GetFileName(assetPath),
                reference = mainAsset != null ? new InstanceReference<Object>(mainAsset) : null,
                children = new List<AssetTreeItem>()
            };

            // Add subassets as children
            foreach (var subAsset in allAssets)
            {
                if (subAsset == mainAsset || subAsset == null) continue;
                if (subAsset.hideFlags.HasFlag(HideFlags.HideInHierarchy)) continue;

                item.children.Add(new AssetTreeItem
                {
                    name = subAsset.name,
                    reference = new InstanceReference<Object>(subAsset),
                    children = null
                });
            }

            if (item.children.Count == 0)
                item.children = null;

            return item;
        }

        private static void EnsureAncestorNodes(string path, string rootPath, Dictionary<string, AssetTreeItem> nodeMap)
        {
            if (path == rootPath || nodeMap.ContainsKey(path)) return;

            var parentPath = GetParentPath(path);
            EnsureAncestorNodes(parentPath, rootPath, nodeMap);

            var folderObj = AssetDatabase.LoadAssetAtPath<Object>(path);
            var node = new AssetTreeItem
            {
                name = Path.GetFileName(path),
                reference = folderObj != null ? new InstanceReference<Object>(folderObj) : null,
                children = new List<AssetTreeItem>()
            };
            nodeMap[path] = node;

            if (nodeMap.TryGetValue(parentPath, out var parent))
                parent.children.Add(node);
        }
    }
}