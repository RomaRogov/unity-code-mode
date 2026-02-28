using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CodeMode.Editor.Tools.Attributes;
using CodeMode.Editor.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Tools.Implementations
{
    public static class EditorTools
    {

        #region EditorOperate
        
        public enum EditorOperation
        {
            Save,
            Play,
            Pause,
            Step,
            Stop,
            Refresh,
            Compile
        }
        
        [UtcpTool("Common editor operations: save, play, pause, step, stop, refresh, compile",
            httpMethod: "POST",
            tags: new[] { "operation", "editor", "scene", "preview", "asset", "refresh" })]
        public static void EditorOperate(EditorOperation operation)
        {
            switch (operation)
            {
                case EditorOperation.Save:
                    UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                    return;

                case EditorOperation.Play:
                    if (!EditorApplication.isPlaying)
                        EditorApplication.isPlaying = true;
                    return;

                case EditorOperation.Pause:
                    EditorApplication.isPaused = !EditorApplication.isPaused;
                    return;

                case EditorOperation.Step:
                    if (EditorApplication.isPlaying && EditorApplication.isPaused)
                        EditorApplication.Step();
                    else
                        throw new Exception("Can only step when paused in play mode");
                    return;

                case EditorOperation.Stop:
                    if (EditorApplication.isPlaying)
                        EditorApplication.isPlaying = false;
                    return;

                case EditorOperation.Refresh:
                    AssetDatabase.Refresh();
                    return;

                case EditorOperation.Compile:
                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                    return;

                default:
                    throw new Exception($"Unknown operation: {operation}");
            }
        }
        
        #endregion

        #region EditorGetLogs
        
        public enum LogOrder
        {
            NewestToOldest,
            OldestToNewest
        }
        
        [Serializable]
        public class EditorGetLogsInput : UtcpInput
        {
            [Tooltip("Number of log entries to retrieve")]
            public int count = 10;

            [Tooltip("Return full stack trace for each log entry")]
            public bool showStack = false;

            [Tooltip("Order of logs")]
            public LogOrder order = LogOrder.NewestToOldest;
        }
        
        [Serializable]
        public class EditorGetLogsOutput
        {
            public List<string> logLines;
        }

        /// <summary>
        /// Get editor log entries
        /// </summary>
        [UtcpTool("Get last N editor log entries",
            httpMethod: "GET",
            tags: new[] { "editor", "logs", "debug", "info" })]
        public static EditorGetLogsOutput EditorGetLogs(EditorGetLogsInput input)
        {
            var logLines = new List<string>();

            var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            if (logEntriesType == null)
                return new EditorGetLogsOutput { logLines = logLines };

            var getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
            var startMethod = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
            var endMethod = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
            var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);

            if (getCountMethod == null || startMethod == null || endMethod == null)
                return new EditorGetLogsOutput { logLines = logLines };

            int totalCount = (int)getCountMethod.Invoke(null, null);
            startMethod.Invoke(null, null);

            try
            {
                var logEntryType = Type.GetType("UnityEditor.LogEntry, UnityEditor");
                if (logEntryType == null)
                    return new EditorGetLogsOutput { logLines = logLines };

                var entry = Activator.CreateInstance(logEntryType);
                var messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
                var modeField = logEntryType.GetField("mode", BindingFlags.Instance | BindingFlags.Public);

                int count = input.count > 0 ? input.count : 10;
                bool newestFirst = input.order == LogOrder.NewestToOldest;

                // Read from end for newest-first
                int startIndex = newestFirst ? Math.Max(0, totalCount - count) : 0;
                int endIndex = newestFirst ? totalCount : Math.Min(count, totalCount);

                for (int i = startIndex; i < endIndex; i++)
                {
                    getEntryMethod?.Invoke(null, new object[] { i, entry });

                    var message = messageField?.GetValue(entry) as string ?? "";
                    var mode = (int)(modeField?.GetValue(entry) ?? 0);
                    var logType = GetLogTypePrefix(mode);

                    // Format: "type: message" similar to reference
                    var line = $"{logType}: {message.Split('\n')[0]}";

                    // Include stack trace if requested
                    if (input.showStack && message.Contains("\n"))
                    {
                        line = $"{logType}: {message}";
                    }

                    logLines.Add(line);
                }

                // Reverse if newest-first (we read from startIndex to end)
                if (newestFirst)
                    logLines.Reverse();
            }
            finally
            {
                endMethod.Invoke(null, null);
            }

            return new EditorGetLogsOutput { logLines = logLines };
        }
        
        private static string GetLogTypePrefix(int mode)
        {
            // Unity ConsoleWindow.Mode flags:
            // Bit 0: Error, Bit 1: Assert, Bit 4: Fatal
            // Bit 8: ScriptingError, Bit 9: ScriptingWarning, Bit 10: ScriptingLog
            // Bit 11: ScriptCompileError, Bit 12: ScriptCompileWarning
            if ((mode & (1 << 0)) != 0) return "error";
            if ((mode & (1 << 1)) != 0) return "assert";
            if ((mode & (1 << 4)) != 0) return "exception";
            if ((mode & (1 << 8)) != 0) return "error";
            if ((mode & (1 << 11)) != 0) return "error";
            if ((mode & (1 << 9)) != 0) return "warn";
            if ((mode & (1 << 12)) != 0) return "warn";
            if ((mode & (1 << 10)) != 0) return "log";
            return "log";
        }
        
        #endregion

        #region EditorGetScenePreview

        [Serializable]
        public class EditorGetScenePreviewInput : UtcpInput
        {
            [Tooltip("Image width in pixels")]
            public int width = 512;

            [Tooltip("Image height in pixels")]
            public int height = 512;

            [Tooltip("JPEG quality (40-100)")]
            public int jpegQuality = 80;

            [Tooltip("Camera world position")]
            public Vector3 cameraPosition;

            [Tooltip("Point the camera looks at")]
            public Vector3 targetPosition;
            
            [Tooltip("Projection type: perspective or orthographic (perspective by default)")]
            [CanBeNull] public bool orthographic = false;
            
            [Tooltip("Orthographic size (only for orthographic projection)")]
            [CanBeNull] public float orthographicSize = 5f;
        }
        
        [UtcpTool("Returns preview image of scene view. IMPORTANT: To visualize the image, you must return the result of this function DIRECTLY as the final value of your code, do NOT wrap it in an object.",
            httpMethod: "GET",
            tags: new[] { "scene", "screenshot", "preview", "inspection", "image" })]
        public static async Task<Base64ImageResult> EditorGetScenePreview(EditorGetScenePreviewInput input)
        {
            var cameraPos = input.cameraPosition;
            var targetPos = input.targetPosition;
            var direction = targetPos - cameraPos;

            if (direction.sqrMagnitude < 0.0001f)
                throw new Exception("cameraPosition and targetPosition must be different");

            var rotation = Quaternion.LookRotation(direction.normalized);
            var width = Mathf.Clamp(input.width > 0 ? input.width : 512, 64, 4096);
            var height = Mathf.Clamp(input.height > 0 ? input.height : 512, 64, 4096);
            var quality = Mathf.Clamp(input.jpegQuality > 0 ? input.jpegQuality : 80, 40, 100);

            // Render via SceneCaptureWindow which provides GUI context for Handles.DrawCamera with skybox
            var bytes = await SceneCaptureWindow.CaptureAsync
                (cameraPos, rotation, width, height, quality, input.orthographic, input.orthographicSize);

            return new Base64ImageResult
            {
                type = "image",
                data = Convert.ToBase64String(bytes),
                mimeType = "image/jpeg"
            };
        }
        
        #endregion
    }
}
