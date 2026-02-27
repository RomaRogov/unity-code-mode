using CodeMode.Editor.Config;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.UI
{
    public class CodeModeSettingsProvider : SettingsProvider
    {
        private UtcpConfig _config;
        private bool _isDirty;

        public CodeModeSettingsProvider(string path, SettingsScope scope)
            : base(path, scope) { }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new CodeModeSettingsProvider("Preferences/Code Mode", SettingsScope.User)
            {
                keywords = new[] { "code", "utcp", "server", "http", "api", "tools", "ai", "mcp" }
            };
            return provider;
        }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            _config = UtcpConfigManager.LoadConfig().Clone();
            _isDirty = false;
        }

        public override void OnDeactivate()
        {
            if (_isDirty)
            {
                UtcpConfigManager.SaveConfig(_config);

                // Restart server if running with new settings
                if (ServerManager.Instance != null && ServerManager.Instance.IsRunning)
                {
                    ServerManager.Instance.RestartServer();
                }
            }
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space(10);

            DrawServerStatus();
            EditorGUILayout.Space(10);

            DrawConfiguration();
            EditorGUILayout.Space(10);

            DrawUtcpConfig();
            EditorGUILayout.Space(10);

            DrawActions();
            EditorGUILayout.Space(10);

            DrawInfo();
        }

        private void DrawServerStatus()
        {
            EditorGUILayout.LabelField("Server Status", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                var isRunning = ServerManager.Instance?.IsRunning ?? false;
                var statusText = isRunning ? "Running" : "Stopped";
                var statusColor = isRunning ? Color.green : Color.gray;

                var oldColor = GUI.color;
                GUI.color = statusColor;
                EditorGUILayout.LabelField("●", GUILayout.Width(20));
                GUI.color = oldColor;

                EditorGUILayout.LabelField(statusText);

                GUILayout.FlexibleSpace();

                if (isRunning)
                {
                    if (GUILayout.Button("Stop", GUILayout.Width(80)))
                    {
                        ServerManager.Instance?.StopServer();
                    }
                    if (GUILayout.Button("Restart", GUILayout.Width(80)))
                    {
                        ServerManager.Instance?.RestartServer();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start", GUILayout.Width(80)))
                    {
                        ServerManager.Instance?.StartServer();
                    }
                }
            }

            if (ServerManager.Instance?.IsRunning == true)
            {
                var actualPort = ServerManager.Instance.ActualPort;
                EditorGUILayout.HelpBox($"Server is running at http://{_config.address}:{actualPort}/", MessageType.Info);
            }
        }

        private void DrawConfiguration()
        {
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            _config.address = EditorGUILayout.TextField(
                new GUIContent("IP Address", "IP address for the UTCP server to bind to"),
                _config.address);
            
            _config.port = EditorGUILayout.IntField(
                new GUIContent("Port", "HTTP port for the UTCP server (0 = auto-select free port)"),
                _config.port);
            _config.port = _config.port < 0 ? 0 : (_config.port > 65535 ? 65535 : _config.port);

            _config.autoStart = EditorGUILayout.Toggle(
                new GUIContent("Auto Start", "Automatically start the server when Unity opens"),
                _config.autoStart);

            _config.logRequests = EditorGUILayout.Toggle(
                new GUIContent("Log Requests", "Log incoming HTTP requests to the console"),
                _config.logRequests);

            _config.serverName = EditorGUILayout.TextField(
                new GUIContent("Server Name", "Template name in UTCP config (for AI assistants to discover)"),
                _config.serverName);

            if (EditorGUI.EndChangeCheck())
            {
                _isDirty = true;
            }

            // Show actual port if running with auto-select
            if (ServerManager.Instance?.IsRunning == true && _config.port == 0)
            {
                EditorGUILayout.HelpBox($"Auto-selected port: {ServerManager.Instance.ActualPort}", MessageType.None);
            }
        }

        private void DrawUtcpConfig()
        {
            EditorGUILayout.LabelField("UTCP Templates", EditorStyles.boldLabel);

            // Unity template status
            var isRegistered = UtcpConfigManager.IsTemplateRegistered(out var currentUrl);
            var templates = UtcpConfigManager.GetAllTemplatesForDisplay();

            using (new EditorGUILayout.HorizontalScope())
            {
                var statusIcon = isRegistered ? "●" : "○";
                var statusColor = isRegistered ? Color.green : Color.yellow;
                var statusText = isRegistered ? "Registered" : "Not registered";

                var oldColor = GUI.color;
                GUI.color = statusColor;
                EditorGUILayout.LabelField(statusIcon, GUILayout.Width(20));
                GUI.color = oldColor;

                EditorGUILayout.LabelField($"Unity Template: {statusText}");

                GUILayout.FlexibleSpace();

                // Quick register/unregister
                if (!isRegistered)
                {
                    if (GUILayout.Button("Register", GUILayout.Width(70)))
                    {
                        UtcpConfigManager.EnsureUnityTemplate(_config.port);
                    }
                }
            }

            // Template count and manager button
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Total Templates: {templates.Length}");

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Manage Templates...", GUILayout.Width(130)))
                {
                    UtcpTemplateManagerWindow.ShowWindow();
                }
            }

            if (isRegistered)
            {
                EditorGUILayout.HelpBox($"URL: {currentUrl}", MessageType.None);
            }
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Server Window"))
                {
                    UtcpServerWindow.ShowWindow();
                }
                
                if (GUILayout.Button("Reset to Defaults"))
                {
                    if (EditorUtility.DisplayDialog("Reset Configuration",
                            "Are you sure you want to reset all CodeMode settings to defaults?", "Reset", "Cancel"))
                    {
                        UtcpConfigManager.ResetConfig();
                        _config = UtcpConfigManager.LoadConfig().Clone();
                        _isDirty = false;
                    }
                }
            }
        }

        private void DrawInfo()
        {
            string configPath = UtcpConfigManager.GetUtcpConfigPath();
            
            EditorGUILayout.LabelField("MCP Integration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("To integrate Code Mode to your AI assistant via MCP, paste following configuration into your MCP config file:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.SelectableLabel("{\n  \"mcpServers\": {\n    \"code-mode\": {\n      " + 
                "\"command\": \"npx\",\n      \"args\": [\"@utcp/code-mode-mcp\"],\n      " + 
                "\"env\": {\n        \"UTCP_CONFIG_FILE\": \""+ configPath + 
                "\"\n      }\n    }\n  }\n}", EditorStyles.textArea, GUILayout.MinHeight(165));
        }
    }
}
