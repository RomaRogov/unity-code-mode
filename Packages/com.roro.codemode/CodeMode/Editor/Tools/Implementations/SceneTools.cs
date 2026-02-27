using System;
using System.Collections.Generic;
using System.Linq;
using CodeMode.Editor.Tools.Attributes;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools.Implementations
{
    [Serializable]
    public class GameObjectReferenceResult
    {
        public InstanceReference<GameObject> reference;
    }

    [Serializable]
    public class GameObjectReferencesResult
    {
        public List<InstanceReference<GameObject>> references;
    }

    public static class SceneTools
    {
        #region GameObjectGetTree

        [UtcpTool("Get the hierarchy tree of specific GameObject or scene root if no reference is provided. Children have recursive structure.",
            httpMethod: "GET",
            tags: new[] { "scene", "graph", "gameobject", "hierarchy", "tree" })]
        public static SceneTreeItem GameObjectGetTree(InstanceReference<GameObject> reference)
        {
            if (reference != null)
            {
                var go = reference.Instance;
                if (!go)
                    throw new Exception($"GameObject not found for reference {reference.id}");
                return BuildGameObjectTree(go);
            }

            // Default: build tree from scene roots
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            return new SceneTreeItem
            {
                path = scene.path,
                name = scene.name,
                active = true,
                components = new List<InstanceReference<Component>>(),
                children = rootObjects.Select(BuildGameObjectTree).ToList()
            };
        }

        #endregion

        #region GameObjectGetAtPath

        [UtcpTool("Get GameObjects at specific path in the scene hierarchy. Usually returns one, but can return multiple with the same name.",
            httpMethod: "GET",
            tags: new[] { "scene", "gameobject", "get", "path", "find", "look", "instance", "hierarchy" })]
        public static GameObjectReferencesResult GameObjectGetAtPath(string hierarchyPath)
        {
            if (string.IsNullOrEmpty(hierarchyPath))
                throw new Exception("hierarchyPath is required");

            var path = hierarchyPath.Trim('/');
            var pathParts = path.Split('/').Where(p => p.Length > 0).ToArray();
            if (pathParts.Length == 0)
                throw new Exception("hierarchyPath is empty");

            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            // First part matches root GameObjects
            List<GameObject> currentNodes = rootObjects
                .Where(go => go.name == pathParts[0])
                .ToList();

            // Walk remaining parts
            for (int i = 1; i < pathParts.Length && currentNodes.Count > 0; i++)
            {
                var nextNodes = new List<GameObject>();
                foreach (var node in currentNodes)
                {
                    foreach (Transform child in node.transform)
                    {
                        if (child.name == pathParts[i])
                            nextNodes.Add(child.gameObject);
                    }
                }
                currentNodes = nextNodes;
            }

            return new GameObjectReferencesResult
            {
                references = currentNodes
                    .Select(go => new InstanceReference<GameObject>(go))
                    .ToList()
            };
        }

        #endregion

        #region GameObjectCreatePrimitive

        public class GameObjectCreatePrimitiveInput : UtcpInput
        {
            [Tooltip("Name for the new GameObject")]
            [CanBeNull] public string name;

            [Tooltip("Primitive geometry type")]
            public PrimitiveType primitiveType;

            [Tooltip("Parent GameObject reference. If not provided, created at scene root.")]
            [CanBeNull] public InstanceReference<GameObject> parentReference;
        }

        [UtcpTool("Create a new GameObject with predefined primitive geometry MeshRenderer. If no parent is specified, root is used. Returns reference to the new GameObject.",
            httpMethod: "POST",
            tags: new[] { "scene", "gameobject", "create", "add", "primitive" })]
        public static GameObjectReferenceResult GameObjectCreatePrimitive(GameObjectCreatePrimitiveInput input)
        {
            var go = GameObject.CreatePrimitive(input.primitiveType);
            go.name = !string.IsNullOrEmpty(input.name) ? input.name : input.primitiveType.ToString();

            if (input.parentReference != null)
            {
                var parent = input.parentReference.Instance;
                if (!parent) throw new Exception("Parent GameObject not found");
                go.transform.SetParent(parent.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {go.name}");

            return new GameObjectReferenceResult { reference = new InstanceReference<GameObject>(go) };
        }

        #endregion

        #region GameObjectCreate

        public class GameObjectCreateInput : UtcpInput
        {
            [Tooltip("Name for the new GameObject")]
            public string name;

            [Tooltip("Parent GameObject reference. If not provided, created at scene root.")]
            [CanBeNull] public InstanceReference<GameObject> parentReference;

            [Tooltip("Asset reference (prefab) to instantiate from")]
            [CanBeNull] public InstanceReference<Object> assetReference;
        }

        [UtcpTool("Create a new GameObject in the scene. If no parent is specified, root is used. Returns reference to the new GameObject.",
            httpMethod: "POST",
            tags: new[] { "scene", "gameobject", "create", "add" })]
        public static GameObjectReferenceResult GameObjectCreate(GameObjectCreateInput input)
        {
            if (string.IsNullOrEmpty(input.name))
                throw new Exception("name is required");

            GameObject go;

            if (input.assetReference != null)
            {
                var asset = input.assetReference.Instance;
                if (!asset)
                    throw new Exception("Asset reference not found");

                if (asset is GameObject prefabAsset)
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                    if (go == null)
                        throw new Exception("Failed to instantiate prefab");
                }
                else
                {
                    throw new Exception($"Asset reference {input.assetReference.id} is not a prefab.");
                }
            }
            else
            {
                go = new GameObject();
            }

            go.name = input.name;

            if (input.parentReference != null)
            {
                var parent = input.parentReference.Instance;
                if (!parent) throw new Exception("Parent GameObject not found");
                go.transform.SetParent(parent.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {go.name}");

            return new GameObjectReferenceResult { reference = new InstanceReference<GameObject>(go) };
        }

        #endregion

        #region GameObjectOperate

        public enum GameObjectOperation
        {
            Move,
            Copy,
            Delete,
            CreatePrefab,
            RevertPrefab,
            ApplyPrefab,
            UnwrapPrefab,
            UnwrapPrefabCompletely,
            OpenPrefab
        }

        public class GameObjectOperateInput : UtcpInput
        {
            public GameObjectOperation operation;
            public InstanceReference<GameObject> reference;

            [Tooltip("New parent for move/copy operations")]
            [CanBeNull] public InstanceReference<GameObject> newParentReference;

            [Tooltip("For CreatePrefab: target asset path for the new prefab")]
            [CanBeNull] public string newPrefabPath;

            [Tooltip("For Move/Copy: target sibling index in parent children array")]
            public int? siblingIndex;
        }
        
        [Serializable]
        public class GameObjectOperateOutput
        {
            public bool success;
            [CanBeNull] public InstanceReference<Object> createdPrefabAssetReference;
            [CanBeNull] public InstanceReference<GameObject> updatedGameObjectReference;
            [CanBeNull] public InstanceReference<GameObject> copiedGameObjectReference;
        }

        [UtcpTool("Perform operation on referenced GameObject, including prefab operations.",
            httpMethod: "POST",
            tags: new[] { "scene", "gameobject", "remove", "move", "copy", "delete", "prefab", "apply", "revert", "unwrap", "create" })]
        public static GameObjectOperateOutput GameObjectOperate(GameObjectOperateInput input)
        {
            var go = input.reference?.Instance;
            if (!go)
                throw new Exception("GameObject reference is required");

            switch (input.operation)
            {
                case GameObjectOperation.Move:
                {
                    if (input.newParentReference == null)
                        throw new Exception("newParentReference required for move");
                    var newParent = input.newParentReference.Instance;
                    if (!newParent)
                        throw new Exception("New parent GameObject not found");

                    Undo.SetTransformParent(go.transform, newParent.transform, $"Move {go.name}");
                    if (input.siblingIndex.HasValue)
                        go.transform.SetSiblingIndex(input.siblingIndex.Value);

                    return new GameObjectOperateOutput { success = true };
                }

                case GameObjectOperation.Copy:
                {
                    var duplicate = Object.Instantiate(go, go.transform.parent);
                    duplicate.name = go.name;
                    Undo.RegisterCreatedObjectUndo(duplicate, $"Copy {go.name}");

                    if (input.newParentReference != null)
                    {
                        var newParent = input.newParentReference.Instance;
                        if (!newParent)
                            throw new Exception("New parent GameObject not found");
                        Undo.SetTransformParent(duplicate.transform, newParent.transform, $"Reparent copy of {go.name}");
                    }

                    if (input.siblingIndex.HasValue)
                        duplicate.transform.SetSiblingIndex(input.siblingIndex.Value);

                    return new GameObjectOperateOutput
                    {
                        success = true,
                        copiedGameObjectReference = new InstanceReference<GameObject>(duplicate)
                    };
                }

                case GameObjectOperation.Delete:
                {
                    Undo.DestroyObjectImmediate(go);
                    return new GameObjectOperateOutput { success = true };
                }

                case GameObjectOperation.CreatePrefab:
                {
                    if (string.IsNullOrEmpty(input.newPrefabPath))
                        throw new Exception("newPrefabPath required for CreatePrefab");

                    var path = input.newPrefabPath;
                    if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                        path += ".prefab";

                    var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.UserAction);
                    if (prefab == null)
                        throw new Exception("Failed to create prefab asset");

                    return new GameObjectOperateOutput
                    {
                        success = true,
                        createdPrefabAssetReference = new InstanceReference<Object>(prefab),
                        updatedGameObjectReference = new InstanceReference<GameObject>(go)
                    };
                }

                case GameObjectOperation.RevertPrefab:
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(go))
                        throw new Exception("GameObject is not a prefab instance");
                    PrefabUtility.RevertPrefabInstance(go, InteractionMode.UserAction);
                    return new GameObjectOperateOutput { success = true };
                }

                case GameObjectOperation.ApplyPrefab:
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(go))
                        throw new Exception("GameObject is not a prefab instance");
                    var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                    if (string.IsNullOrEmpty(prefabPath))
                        throw new Exception("Could not resolve prefab asset path");
                    PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
                    return new GameObjectOperateOutput { success = true };
                }

                case GameObjectOperation.UnwrapPrefab:
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(go))
                        throw new Exception("GameObject is not a prefab instance");
                    var outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    PrefabUtility.UnpackPrefabInstance(outermost, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                    return new GameObjectOperateOutput { success = true };
                }

                case GameObjectOperation.UnwrapPrefabCompletely:
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(go))
                        throw new Exception("GameObject is not a prefab instance");
                    var outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                    PrefabUtility.UnpackPrefabInstance(outermost, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                    return new GameObjectOperateOutput { success = true };
                }

                case GameObjectOperation.OpenPrefab:
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(go))
                        throw new Exception("GameObject is not a prefab instance");
                    var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                    if (string.IsNullOrEmpty(prefabAssetPath))
                        throw new Exception("Could not resolve prefab asset path");
                    var prefabAsset = AssetDatabase.LoadMainAssetAtPath(prefabAssetPath);
                    AssetDatabase.OpenAsset(prefabAsset);
                    return new GameObjectOperateOutput { success = true };
                }

                default:
                    throw new Exception($"Unknown operation: {input.operation}");
            }
        }

        #endregion

        #region Helper Methods

        private static SceneTreeItem BuildGameObjectTree(GameObject go)
        {
            var item = new SceneTreeItem
            {
                path = GetGameObjectPath(go),
                name = go.name,
                reference = new InstanceReference<GameObject>(go),
                active = go.activeInHierarchy,
                components = go.GetComponents<Component>()
                    .Where(c => c != null)
                    .Select(c => new InstanceReference<Component>(c))
                    .ToList(),
                children = new List<SceneTreeItem>()
            };

            foreach (Transform child in go.transform)
            {
                item.children.Add(BuildGameObjectTree(child.gameObject));
            }

            return item;
        }

        private static string GetGameObjectPath(GameObject go)
        {
            var path = "/" + go.name;
            var current = go.transform.parent;

            while (current != null)
            {
                path = "/" + current.name + path;
                current = current.parent;
            }

            return path;
        }

        #endregion
    }
}