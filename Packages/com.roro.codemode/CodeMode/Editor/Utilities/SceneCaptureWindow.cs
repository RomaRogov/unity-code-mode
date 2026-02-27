using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Utilities
{
    /// <summary>
    /// Helper EditorWindow that provides a proper GUI context for Handles.DrawCamera.
    /// Renders scene with grid and gizmos into a RenderTexture (no screen visibility needed).
    /// </summary>
    public class SceneCaptureWindow : EditorWindow
    {
        private static AutoResetUniTaskCompletionSource<byte[]> _tcs;
        private static Vector3 _cameraPos;
        private static Quaternion _rotation;
        private static int _width, _height, _quality;
        private static bool _orthographic;
        private static float _orthographicSize;

        public static UniTask<byte[]> CaptureAsync(
            Vector3 cameraPos, Quaternion rotation, int width, int height, int quality, bool orthographic, float orthographicSize)
        {
            _tcs = AutoResetUniTaskCompletionSource<byte[]>.Create();
            _cameraPos = cameraPos;
            _rotation = rotation;
            _width = width;
            _height = height;
            _quality = quality;
            _orthographic = orthographic;
            _orthographicSize = orthographicSize;

            var window = CreateInstance<SceneCaptureWindow>();
            window.ShowUtility();
            window.minSize = Vector2.one;
            window.position = new Rect(0, 0, _width, _height);
            window.Repaint();

            return _tcs.Task;
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            
            Camera cam = null;
            
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
                cam = sceneView.camera;
            
            if (cam == null)
            {
                throw new NullReferenceException("Camera not found");
            }
            
            // Store original camera settings
            Vector3 prevPos = cam.transform.position;
            Quaternion prevRot = cam.transform.rotation;
            float prevAspect = cam.aspect;
            bool prevOrthographic = cam.orthographic;
            float prevOrthographicSize = cam.orthographicSize;
            
            // Set camera transform
            cam.transform.position = _cameraPos;
            cam.transform.rotation = _rotation;
            cam.aspect = (float)_width / _height;
            cam.orthographic = _orthographic;
            cam.orthographicSize = _orthographicSize;

            try
            {
                var rt = RenderTexture.GetTemporary(_width, _height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;

                var rect = new Rect(0, 0, _width, _height);
                
                cam.Render();
                
                // Render with skybox
                Handles.DrawCamera(rect, cam, sceneView.cameraMode.drawMode, true);

                sceneView.Repaint();
                SceneView.RepaintAll();

                // Read pixels from the RenderTexture
                RenderTexture.active = rt;
                var tex = new Texture2D(_width, _height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
                tex.Apply();

                cam.targetTexture = null;
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);

                var bytes = tex.EncodeToJPG(_quality);
                DestroyImmediate(tex);

                _tcs.TrySetResult(bytes);
            }
            catch (Exception e)
            {
                _tcs.TrySetException(e);
            }
            finally
            {
                Close();
                // Restore all original camera settings
                cam.transform.position = prevPos;
                cam.transform.rotation = prevRot;
                cam.aspect = prevAspect;
                cam.orthographic = prevOrthographic;
                cam.orthographicSize = prevOrthographicSize;
                cam.enabled = true;
                SceneView.lastActiveSceneView.ResetCameraSettings();
                cam.Render();
                sceneView.Repaint();
                SceneView.RepaintAll();
            }
        }
    }
}