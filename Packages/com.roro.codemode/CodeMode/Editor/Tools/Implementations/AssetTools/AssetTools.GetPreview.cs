using System;
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
        public class AssetGetPreviewInput : UtcpInput
        {
            public InstanceReference<Object> reference;

            [Tooltip("JPEG quality (40-100)")]
            [CanBeNull] public int jpegQuality = 80;
        }

        [UtcpTool("Returns preview image of the asset (Prefab, Texture, Model, Material, etc.) as base64 JPEG.",
            httpMethod: "GET",
            tags: new[] { "asset", "preview", "screenshot", "image" })]
        public static async UniTask<Base64ImageResult> AssetGetPreview(AssetGetPreviewInput input)
        {
            var instance = input.reference?.Instance;
            if (!instance)
                throw new Exception("asset reference is required");

            var jpegQuality = Mathf.Clamp(input.jpegQuality > 0 ? input.jpegQuality : 80, 40, 100);

            // Try editor asset preview, yield frames while Unity generates it
            Texture2D preview = AssetPreview.GetAssetPreview(instance);
            for (int i = 0; i < 100 && preview == null; i++)
            {
                if (!AssetPreview.IsLoadingAssetPreview(instance.GetInstanceID()))
                    break;
                await UniTask.DelayFrame(1);
                preview = AssetPreview.GetAssetPreview(instance);
            }

            // Fallback to mini thumbnail
            if (preview == null)
                preview = AssetPreview.GetMiniThumbnail(instance);

            if (preview == null)
                throw new Exception("Failed to generate preview for this asset");

            // Some textures (built-in icons, etc.) are not readable — copy to a readable texture via RenderTexture
            byte[] jpgBytes;
            try
            {
                jpgBytes = preview.EncodeToJPG(jpegQuality);
            }
            catch (Exception)
            {
                var rt = RenderTexture.GetTemporary(preview.width, preview.height);
                Graphics.Blit(preview, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var readable = new Texture2D(preview.width, preview.height);
                readable.ReadPixels(new UnityEngine.Rect(0, 0, preview.width, preview.height), 0, 0);
                readable.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                jpgBytes = readable.EncodeToJPG(jpegQuality);
                Object.DestroyImmediate(readable);
            }

            return new Base64ImageResult
            {
                type = "image",
                data = Convert.ToBase64String(jpgBytes),
                mimeType = "image/jpeg"
            };
        }
    }
}