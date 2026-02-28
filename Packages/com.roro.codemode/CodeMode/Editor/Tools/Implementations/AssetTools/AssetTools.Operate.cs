using System;
using System.Threading.Tasks;
using CodeMode.Editor.Tools.Attributes;
using CodeMode.Editor.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations.AssetTools
{
    public static partial class AssetTools
    {
        public enum AssetOperation
        {
            Move,
            Copy,
            Delete,
            Open,
            Refresh,
            Reimport
        }

        [Serializable]
        public class AssetOperateInput : UtcpInput
        {
            public AssetOperation operation;
            public InstanceReference<Object> reference;

            [Tooltip("Target path (for move/copy operations)")]
            public string targetAssetPath;

            [Tooltip("Additional options for the operation")]
            [CanBeNull] public AssetOperationOptions options;
        }

        [UtcpTool("Perform operations on assets: move, copy, delete, open, refresh, reimport",
            httpMethod: "POST",
            tags: new[] { "asset", "operate", "move", "copy", "delete", "open", "refresh", "reimport" })]
        public static async Task AssetOperate(AssetOperateInput input)
        {
            var instance = input.reference?.Instance;
            if (!instance)
                throw new Exception("Asset reference is required");

            var sourcePath = AssetDatabase.GetAssetPath(instance);
            if (string.IsNullOrEmpty(sourcePath))
                throw new Exception("Could not resolve asset path from reference");

            switch (input.operation)
            {
                case AssetOperation.Move:
                    if (string.IsNullOrEmpty(input.targetAssetPath))
                        throw new Exception("targetAssetPath is required for move operation");
                    var moveDest = ResolveTargetPath(NormalizePath(input.targetAssetPath), input.options);
                    var moveResult = AssetDatabase.MoveAsset(sourcePath, moveDest);
                    if (!string.IsNullOrEmpty(moveResult))
                        throw new Exception(moveResult);
                    return;

                case AssetOperation.Copy:
                    if (string.IsNullOrEmpty(input.targetAssetPath))
                        throw new Exception("targetAssetPath is required for copy operation");
                    var copyDest = ResolveTargetPath(NormalizePath(input.targetAssetPath), input.options);
                    if (!AssetDatabase.CopyAsset(sourcePath, copyDest))
                        throw new Exception($"Failed to copy asset from {sourcePath}");
                    return;

                case AssetOperation.Delete:
                    if (!AssetDatabase.DeleteAsset(sourcePath))
                        throw new Exception($"Failed to delete asset at {sourcePath}");
                    return;

                case AssetOperation.Open:
                    AssetDatabase.OpenAsset(instance);
                    return;

                case AssetOperation.Refresh:
                    AssetDatabase.Refresh();
                    await EditorAsync.Yield();
                    return;

                case AssetOperation.Reimport:
                    AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceUpdate);
                    await EditorAsync.Yield();
                    return;

                default:
                    throw new Exception($"Unknown operation: {input.operation}");
            }
        }
    }
}