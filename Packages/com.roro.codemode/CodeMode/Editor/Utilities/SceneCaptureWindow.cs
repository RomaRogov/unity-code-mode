using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Utilities
{
    public class SceneCaptureWindow : SceneView
    {
        private static TaskCompletionSource<byte[]> _tcs;
        private static int _quality;
        
        private bool _captured;
        
        // The tab bar (title) height in logical pixels.
        private const float TabBarHeight = 26f;

        public static Task<byte[]> CaptureAsync(
            Vector3 cameraPos, Quaternion rotation, int width, int height, int quality,
            bool orthographic, float orthographicSize)
        {
            _tcs = new TaskCompletionSource<byte[]>();
            _quality = quality;
            
            float dpi = EditorGUIUtility.pixelsPerPoint;

            var window = GetWindow<SceneCaptureWindow>();
            window._captured = false;
            window.position = new Rect(0, 0, width / dpi, height / dpi + TabBarHeight);
            window.orthographic = orthographic;
            window.LookAtDirect( cameraPos, rotation, Mathf.Max(0.01f, orthographicSize));

            window.ShowModal();
            return _tcs.Task;
        }

        protected override void OnSceneGUI()
        {
            if (_captured)
            {
                base.OnSceneGUI();
                return;
            }
            
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            
            RenderTexture.active = camera.targetTexture;
            
            base.OnSceneGUI();
            
            Texture2D tex = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            tex.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            
            _captured = true;
            _tcs.SetResult(tex.EncodeToJPG(_quality));
            Close();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (!_captured)
                _tcs?.TrySetException(new Exception("SceneCaptureWindow closed before capture completed"));
        }
    }
}