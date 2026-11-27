using System;
using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.Tools
{
    /// <summary>
    /// Reference to a Unity object instance by ID
    /// </summary>
    [Serializable]
    public class InstanceReference<T> where T : Object
    {
        public readonly string id;
        public readonly string type;
        
        [JsonIgnore]
        public T Instance
        {
            get
            {
                if (ObjectIds.FromId(id) is T obj)
                {
                    return obj;
                }
                Debug.LogWarning($"InstanceReference: Failed to resolve instance for ID {id} and type {type}");
                return null;
            }
        }

        public InstanceReference(T obj)
        {
            if (obj == null)
            {
                return;
            }
            id = ObjectIds.ToId(obj);
            type = obj.GetType().Name;
        }
        
        [JsonConstructor]
        public InstanceReference(string id, string type)
        {
            this.id = id;
            this.type = type;
        }
    }

    [Serializable]
    public class Base64ImageResult
    {
        public string type;
        public string data;
        public string mimeType;
    }

    [Serializable]
    public class SceneTreeItem
    {
        [CanBeNull] public string path;
        public InstanceReference<GameObject> reference;
        public string name;
        public bool active;
        public List<InstanceReference<Component>> components;
        public List<SceneTreeItem> children;
    }
    
    [Serializable]
    public class AssetTreeItem
    {
        [CanBeNull] public string filesystemPath;
        public InstanceReference<Object> reference;
        public string name;
        public List<AssetTreeItem> children;
    }
}
