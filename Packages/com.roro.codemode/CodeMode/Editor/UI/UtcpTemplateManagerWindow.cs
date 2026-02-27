using CodeMode.Editor.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.UI
{
    /// <summary>
    /// Window for managing UTCP call templates in ~/.utcp_config.json
    /// Allows adding custom templates for Code Mode interactions
    /// </summary>
    public class UtcpTemplateManagerWindow : EditorWindow
    {
        private Vector2 _templatesScroll;
        private Vector2 _jsonInputScroll;
        private string _newTemplateJson = "";
        private string _errorMessage = "";
        private TemplateDisplayInfo[] _templates;
        private int _expandedIndex = -1;

        [MenuItem("CodeMode/UTCP Templates")]
        public static void ShowWindow()
        {
            var window = GetWindow<UtcpTemplateManagerWindow>("UTCP Templates");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshTemplates();
        }

        private void OnFocus()
        {
            RefreshTemplates();
        }

        private void RefreshTemplates()
        {
            _templates = UtcpConfigManager.GetAllTemplatesForDisplay();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawConfigPath();
            EditorGUILayout.Space(10);

            DrawTemplateList();
            EditorGUILayout.Space(10);

            DrawAddTemplateSection();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("UTCP Call Templates", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                {
                    RefreshTemplates();
                }
            }

            EditorGUILayout.HelpBox(
                "Call templates allow Code Mode server to discover and connect to different tools " +
                "defined with UTCP protocol. Add templates by pasting valid JSON below.",
                MessageType.Info);
            
            string linkText = "UTCP protocol documentation";
            GUIStyle linkStyle = new GUIStyle(EditorStyles.linkLabel);
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(linkText), linkStyle);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, linkText, linkStyle))
            {
                Application.OpenURL("https://utcp.io/protocols");
            }
        }

        private void DrawConfigPath()
        {
            EditorGUILayout.LabelField("Config File", EditorStyles.boldLabel);

            var configPath = UtcpConfigManager.GetUtcpConfigPath();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField(configPath);

                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    var newPath = EditorUtility.OpenFilePanel("Select UTCP Config",
                        System.IO.Path.GetDirectoryName(configPath), "json");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        UtcpConfigManager.SetUtcpConfigPath(newPath);
                        RefreshTemplates();
                    }
                }

                if (GUILayout.Button("Open", GUILayout.Width(50)))
                {
                    if (System.IO.File.Exists(configPath))
                    {
                        EditorUtility.RevealInFinder(configPath);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("File Not Found",
                            $"Config file does not exist at:\n{configPath}\n\nAdd a template to create it.", "OK");
                    }
                }
            }
        }

        private void DrawTemplateList()
        {
            EditorGUILayout.LabelField($"Templates ({_templates?.Length ?? 0})", EditorStyles.boldLabel);

            if (_templates == null || _templates.Length == 0)
            {
                EditorGUILayout.HelpBox("No templates registered. Add one below.", MessageType.None);
                return;
            }

            using (var scroll = new EditorGUILayout.ScrollViewScope(_templatesScroll, GUILayout.MinHeight(150), GUILayout.MaxHeight(300)))
            {
                _templatesScroll = scroll.scrollPosition;

                for (int i = 0; i < _templates.Length; i++)
                {
                    DrawTemplateItem(_templates[i], i);
                }
            }
        }

        private void DrawTemplateItem(TemplateDisplayInfo template, int index)
        {
            var config = UtcpConfigManager.LoadConfig();
            var isUnityTemplate = template.name == config.serverName;
            var isExpanded = _expandedIndex == index;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // Header row
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Expand/collapse button
                    var foldoutContent = isExpanded ? "▼" : "►";
                    if (GUILayout.Button(foldoutContent, GUILayout.Width(20)))
                    {
                        _expandedIndex = isExpanded ? -1 : index;
                    }

                    // Template name
                    var nameStyle = new GUIStyle(EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(template.name, nameStyle);

                    // Type badge
                    var badgeStyle = new GUIStyle(EditorStyles.miniLabel);
                    badgeStyle.normal.textColor = Color.gray;
                    GUILayout.Label($"[{template.call_template_type}]", badgeStyle, GUILayout.Width(50));

                    GUILayout.FlexibleSpace();

                    // Unity template indicator
                    if (isUnityTemplate)
                    {
                        var unityBadge = new GUIStyle(EditorStyles.miniLabel);
                        unityBadge.normal.textColor = new Color(0.2f, 0.6f, 1f);
                        GUILayout.Label("Unity", unityBadge, GUILayout.Width(40));
                    }

                    // Delete button (not for Unity template)
                    GUI.enabled = !isUnityTemplate;
                    if (GUILayout.Button("×", GUILayout.Width(22)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Template",
                            $"Remove template '{template.name}'?", "Remove", "Cancel"))
                        {
                            RemoveTemplate(template.name);
                        }
                    }
                    GUI.enabled = true;
                }

                // URL preview (if available)
                if (!string.IsNullOrEmpty(template.url))
                {
                    var urlStyle = new GUIStyle(EditorStyles.miniLabel);
                    urlStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    EditorGUILayout.LabelField(template.url, urlStyle);
                }

                // Expanded details - show raw JSON
                if (isExpanded)
                {
                    EditorGUILayout.Space(5);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField("Raw JSON:", EditorStyles.miniLabel);

                        // Format the JSON for display
                        var formattedJson = FormatJson(template.rawJson);
                        var textStyle = new GUIStyle(EditorStyles.textArea);
                        textStyle.wordWrap = true;

                        var height = Mathf.Min(200, Mathf.Max(80, formattedJson.Split('\n').Length * 14));
                        EditorGUILayout.TextArea(formattedJson, textStyle, GUILayout.Height(height));

                        if (GUILayout.Button("Copy JSON", GUILayout.Width(100)))
                        {
                            EditorGUIUtility.systemCopyBuffer = formattedJson;
                            Debug.Log($"[UTCP] Copied template JSON for '{template.name}'");
                        }
                    }
                }
            }
        }

        private void DrawAddTemplateSection()
        {
            EditorGUILayout.LabelField("Add Template", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Paste template JSON:", EditorStyles.miniLabel);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_jsonInputScroll, GUILayout.Height(100)))
            {
                _jsonInputScroll = scroll.scrollPosition;
                _newTemplateJson = EditorGUILayout.TextArea(_newTemplateJson, GUILayout.ExpandHeight(true));
            }

            // Error message
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
            }

            EditorGUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Template", GUILayout.Width(120)))
                {
                    AddTemplate();
                }
            }
        }

        private void AddTemplate()
        {
            _errorMessage = "";

            if (string.IsNullOrWhiteSpace(_newTemplateJson))
            {
                _errorMessage = "JSON is required";
                return;
            }

            // Try to validate it's at least valid JSON structure
            var trimmed = _newTemplateJson.Trim();
            if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
            {
                _errorMessage = "Invalid JSON: must be a JSON object";
                return;
            }

            if (UtcpConfigManager.AddTemplateRaw(_newTemplateJson, out var error))
            {
                _newTemplateJson = "";
                _errorMessage = "";
                RefreshTemplates();
            }
            else
            {
                _errorMessage = error;
            }
        }

        private void RemoveTemplate(string name)
        {
            UtcpConfigManager.RemoveTemplateByName(name);
            RefreshTemplates();
        }

        /// <summary>
        /// Format JSON for display using Newtonsoft.Json
        /// </summary>
        private string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            try
            {
                var obj = JToken.Parse(json);
                return obj.ToString(Formatting.Indented);
            }
            catch
            {
                return json; // Return as-is if parsing fails
            }
        }
    }
}
