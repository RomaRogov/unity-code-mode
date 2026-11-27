using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Utilities
{
    /// <summary>
    /// Object id helpers. Unity 6.4+ replaced int instance IDs with EntityId.
    /// Ids are exchanged as strings.
    /// </summary>
    public static class ObjectIds
    {
        public static string ToId(Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return EntityId.ToULong(obj.GetEntityId()).ToString();
#else
            return obj.GetInstanceID().ToString();
#endif
        }

        public static Object FromId(string id)
        {
#if UNITY_6000_4_OR_NEWER
            return ulong.TryParse(id, out var value) ? EditorUtility.EntityIdToObject(EntityId.FromULong(value)) : null;
#else
            return int.TryParse(id, out var value) ? EditorUtility.InstanceIDToObject(value) : null;
#endif
        }

        public static bool IsLoadingAssetPreview(Object obj)
        {
#if UNITY_6000_4_OR_NEWER
            return AssetPreview.IsLoadingAssetPreview(obj.GetEntityId());
#else
            return AssetPreview.IsLoadingAssetPreview(obj.GetInstanceID());
#endif
        }
    }
}
