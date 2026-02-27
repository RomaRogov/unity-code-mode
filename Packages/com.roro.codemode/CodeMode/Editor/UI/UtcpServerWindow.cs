using System;
using System.Collections.Generic;
using CodeMode.Editor.Config;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.UI
{
    public class UtcpServerWindow : EditorWindow
    {
        private Vector2 _logScrollPosition;
        private Vector2 _toolsScrollPosition;
        private List<LogEntry> _logs = new List<LogEntry>();
        private const int MaxLogs = 500;
        private int _selectedTab;
        private readonly string[] _tabNames = { "Server", "Tools", "Logs" };

        [MenuItem("CodeMode/UTCP Server")]
        public static void ShowWindow()
        {
            var window = GetWindow<UtcpServerWindow>();
            window.titleContent = new GUIContent("UTCP Server");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [MenuItem("CodeMode/Settings")]
        public static void OpenSettings()
        {
            SettingsService.OpenUserPreferences("Preferences/Code Mode");
        }

        private void OnEnable()
        {
            if (ServerManager.Instance != null)
            {
                ServerManager.Instance.OnLog -= OnServerLog;
                ServerManager.Instance.OnLog += OnServerLog;
            }
        }

        private void OnDisable()
        {
            if (ServerManager.Instance != null)
            {
                ServerManager.Instance.OnLog -= OnServerLog;
            }
        }

        private void OnServerLog(string message)
        {
            _logs.Add(new LogEntry { time = DateTime.Now, message = message });

            while (_logs.Count > MaxLogs)
            {
                _logs.RemoveAt(0);
            }

            Repaint();
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            EditorGUILayout.Space(5);

            switch (_selectedTab)
            {
                case 0:
                    DrawServerTab();
                    break;
                case 1:
                    DrawToolsTab();
                    break;
                case 2:
                    DrawLogsTab();
                    break;
            }
        }

        private void DrawServerTab()
        {
            var manager = ServerManager.Instance;
            var isRunning = manager?.IsRunning ?? false;
            var config = UtcpConfigManager.LoadConfig();

            // Status
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Status:", GUILayout.Width(60));

                var statusText = isRunning ? "Running" : "Stopped";
                var statusColor = isRunning ? new Color(0.2f, 0.8f, 0.2f) : Color.gray;

                var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = statusColor } };
                EditorGUILayout.LabelField(statusText, style);
            }

            if (isRunning)
            {
                var actualPort = manager?.ActualPort ?? config.port;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Endpoint:", GUILayout.Width(60));
                    var url = $"http://localhost:{actualPort}/utcp";
                    EditorGUILayout.SelectableLabel(url, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    if (GUILayout.Button("Copy", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.systemCopyBuffer = url;
                    }
                }
            }

            EditorGUILayout.Space(10);

            // Controls
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !isRunning;
                if (GUILayout.Button("Start", GUILayout.Height(30)))
                {
                    manager?.StartServer();
                }

                GUI.enabled = isRunning;
                if (GUILayout.Button("Stop", GUILayout.Height(30)))
                {
                    manager?.StopServer();
                }

                if (GUILayout.Button("Restart", GUILayout.Height(30)))
                {
                    manager?.RestartServer();
                }
                GUI.enabled = true;
            }

            EditorGUILayout.Space(10);

            // Show actual port if running and different from config
            if (isRunning && manager != null && (config.port == 0 || config.port != manager.ActualPort))
            {
                EditorGUILayout.HelpBox($"Server is running on port {manager.ActualPort}", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Open Preferences"))
            {
                SettingsService.OpenUserPreferences("Preferences/Code Mode");
            }
        }

        private void DrawToolsTab()
        {
            var registry = ServerManager.Instance?.ToolRegistry;
            if (registry == null)
            {
                EditorGUILayout.HelpBox("Tool registry not available. Start the server first.", MessageType.Info);
                return;
            }

            var tools = registry.Tools;
            EditorGUILayout.LabelField($"Registered Tools: {tools.Count}", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            _toolsScrollPosition = EditorGUILayout.BeginScrollView(_toolsScrollPosition);

            foreach (var kvp in tools)
            {
                var tool = kvp.Value;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(tool.Name, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(tool.HttpMethod, GUILayout.Width(50));
                    }

                    EditorGUILayout.LabelField(tool.Description, EditorStyles.wordWrappedMiniLabel);

                    if (tool.Tags != null && tool.Tags.Length > 0)
                    {
                        EditorGUILayout.LabelField($"Tags: {string.Join(", ", tool.Tags)}", EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawLogsTab()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Logs ({_logs.Count})", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    _logs.Clear();
                }
            }

            EditorGUILayout.Space(5);

            _logScrollPosition = EditorGUILayout.BeginScrollView(_logScrollPosition);

            foreach (var log in _logs)
            {
                var timeStr = log.time.ToString("HH:mm:ss");
                EditorGUILayout.LabelField($"[{timeStr}] {log.message}", EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private struct LogEntry
        {
            public DateTime time;
            public string message;
        }
    }
}
