using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeMode.Editor.Tools.Attributes;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Tools.Implementations
{
    [Serializable]
    public class ComponentTypesResult
    {
        public List<string> componentTypes;
    }

    [Serializable]
    public class ComponentReferencesResult
    {
        public List<InstanceReference<Component>> references;
    }

    [Serializable]
    public class ComponentReferenceResult
    {
        public InstanceReference<Component> reference;
    }

    public static class ComponentTools
    {
        #region GameObjectGetAvailableComponentTypes

        public class GameObjectGetAvailableComponentTypesInput : UtcpInput
        {
            [Tooltip("Whether to include internal engine components")]
            public bool includeInternal = false;

            [Tooltip("Optional filter string to match component types or categories (case-insensitive substring match)")]
            public string filter;
        }

        [UtcpTool("Get list of globally available component types (class names) that can be added via Add Component menu.",
            httpMethod: "GET",
            tags: new[] { "scene", "gameobject", "component", "types", "inspection" })]
        public static ComponentTypesResult GameObjectGetAvailableComponentTypes(GameObjectGetAvailableComponentTypesInput input)
        {
            var componentTypes = TypeCache.GetTypesDerivedFrom<Component>()
                .Where(IsAddableComponent);

            if (!input.includeInternal)
            {
                componentTypes = componentTypes.Where(t =>
                    t.Namespace == null ||
                    (!t.Namespace.StartsWith("UnityEngine.Internal") &&
                     !t.Namespace.StartsWith("UnityEditor.Internal") &&
                     !t.Namespace.StartsWith("UnityEditor")));
            }

            if (!string.IsNullOrEmpty(input.filter))
            {
                var lowerFilter = input.filter.ToLowerInvariant();
                componentTypes = componentTypes.Where(t =>
                    (t.FullName ?? t.Name).ToLowerInvariant().Contains(lowerFilter));
            }

            var names = componentTypes
                .Select(t => t.FullName ?? t.Name)
                .OrderBy(n => n)
                .ToList();

            return new ComponentTypesResult { componentTypes = names };
        }

        /// <summary>
        /// Unity native base types that are NOT abstract in C# but are hidden
        /// from the Add Component menu. These are engine foundation types that
        /// should never be added directly.
        /// </summary>
        private static readonly HashSet<Type> HiddenBaseTypes = new HashSet<Type>
        {
            typeof(Component),
            typeof(Behaviour),
            typeof(MonoBehaviour),
            typeof(Transform),
            typeof(Collider),
            typeof(Collider2D),
            typeof(Renderer),
            typeof(Joint),
            typeof(Joint2D),
            typeof(AnchoredJoint2D),
            typeof(Effector2D),
            typeof(GridLayout),
            typeof(PhysicsUpdateBehaviour2D),
            typeof(AudioBehaviour),
        };

        /// <summary>
        /// Mirrors Unity's Add Component menu filtering rules.
        /// </summary>
        private static bool IsAddableComponent(Type t)
        {
            if (t.IsAbstract || t.IsGenericTypeDefinition) return false;
            if (!t.IsPublic && !t.IsNestedPublic) return false;

            // Skip known Unity base types not in Add Component menu
            if (HiddenBaseTypes.Contains(t)) return false;

            // Skip obsolete types
            if (t.GetCustomAttribute<ObsoleteAttribute>() != null) return false;

            // [AddComponentMenu("")] explicitly hides the type
            var menuAttr = t.GetCustomAttribute<AddComponentMenu>();
            if (menuAttr != null)
            {
                var path = menuAttr.componentMenu;
                if (string.IsNullOrWhiteSpace(path)) return false;
            }

            return true;
        }

        #endregion

        #region GameObjectComponentsGet

        public class GameObjectComponentsGetInput : UtcpInput
        {
            [Tooltip("Reference to the GameObject")]
            public InstanceReference<GameObject> reference;

            [Tooltip("Filter by component type name")]
            [CanBeNull] public string componentType;
        }

        [UtcpTool("Get components of specific type on a GameObject. If componentType is not provided, returns all components.",
            httpMethod: "GET",
            tags: new[] { "scene", "gameobject", "component", "get", "inspection" })]
        public static ComponentReferencesResult GameObjectComponentsGet(GameObjectComponentsGetInput input)
        {
            var go = input.reference?.Instance;
            if (!go)
                throw new Exception("GameObject reference is required");

            var components = go.GetComponents<Component>().Where(c => c != null);

            if (!string.IsNullOrEmpty(input.componentType))
            {
                components = components.Where(c =>
                    c.GetType().Name.Equals(input.componentType, StringComparison.OrdinalIgnoreCase) ||
                    c.GetType().FullName?.Contains(input.componentType) == true);
            }

            var references = components
                .Select(c => new InstanceReference<Component>(c))
                .ToList();

            if (references.Count == 0)
                throw new Exception($"Components{(string.IsNullOrEmpty(input.componentType) ? "" : $" of type {input.componentType}")} not found on GameObject");

            return new ComponentReferencesResult { references = references };
        }

        #endregion

        #region GameObjectComponentAdd

        public class GameObjectComponentAddInput : UtcpInput
        {
            [Tooltip("Reference to the GameObject")]
            public InstanceReference<GameObject> reference;

            [Tooltip("Component type name to add")]
            public string componentType;
        }

        [UtcpTool("Add a component to a referenced GameObject, returns reference to the new component",
            httpMethod: "POST",
            tags: new[] { "scene", "gameobject", "component", "add" })]
        public static ComponentReferenceResult GameObjectComponentAdd(GameObjectComponentAddInput input)
        {
            var go = input.reference?.Instance;
            if (!go)
                throw new Exception("GameObject reference is required");

            if (string.IsNullOrEmpty(input.componentType))
                throw new Exception("componentType is required");

            var type = FindComponentType(input.componentType);
            if (type == null)
                throw new Exception($"Component type not found: {input.componentType}");

            Undo.RecordObject(go, $"Add {type.Name}");

            var component = go.AddComponent(type);
            if (component == null)
                throw new Exception($"Failed to add component {input.componentType}");

            Undo.RegisterCreatedObjectUndo(component, $"Add {type.Name}");

            return new ComponentReferenceResult
            {
                reference = new InstanceReference<Component>(component)
            };
        }

        #endregion

        #region GameObjectComponentRemove

        [UtcpTool("Remove referenced component from GameObject it is attached to.",
            httpMethod: "POST",
            tags: new[] { "scene", "gameobject", "component", "remove", "delete" })]
        public static void GameObjectComponentRemove(InstanceReference<GameObject> reference)
        {
            var component = reference?.Instance;
            if (!component)
                throw new Exception("Component reference is required");

            Undo.DestroyObjectImmediate(component);
        }

        #endregion

        #region Helper Methods

        private static Type FindComponentType(string typeName)
        {
            var addable = TypeCache.GetTypesDerivedFrom<Component>().Where(IsAddableComponent);

            // Try exact match first
            var exactMatch = addable
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) ||
                                     t.FullName?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true);

            if (exactMatch != null)
                return exactMatch;

            // Try partial match
            return addable
                .FirstOrDefault(t => t.Name.Contains(typeName) || t.FullName?.Contains(typeName) == true);
        }

        #endregion
    }
}