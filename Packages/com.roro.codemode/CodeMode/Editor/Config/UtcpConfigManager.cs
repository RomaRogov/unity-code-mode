using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Config
{
    /// <summary>
    /// Manages both Unity-specific settings (EditorPrefs) and global UTCP config (~/.utcp_config.json)
    /// </summary>
    public static class UtcpConfigManager
    {
        private const string EditorPrefsKey = "UtcpServer_Config";

        private static UtcpConfig _cachedConfig;

        /// <summary>
        /// Get the path to the global UTCP config file
        /// </summary>
        public static string GetUtcpConfigPath()
        {
            var config = LoadConfig();
            if (!string.IsNullOrEmpty(config.utcpConfigPath))
                return config.utcpConfigPath;

            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, ".utcp_config.json");
        }

        /// <summary>
        /// Set a custom path for the UTCP config file
        /// </summary>
        public static void SetUtcpConfigPath(string path)
        {
            var config = LoadConfig().Clone();
            config.utcpConfigPath = path;
            SaveConfig(config);
        }

        #region Unity Settings (EditorPrefs)

        public static UtcpConfig LoadConfig()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            var json = EditorPrefs.GetString(EditorPrefsKey, null);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _cachedConfig = JsonUtility.FromJson<UtcpConfig>(json);
                    return _cachedConfig;
                }
                catch
                {
                    // Invalid JSON, return default
                }
            }

            _cachedConfig = new UtcpConfig();
            return _cachedConfig;
        }

        public static void SaveConfig(UtcpConfig config)
        {
            _cachedConfig = config;
            var json = JsonUtility.ToJson(config, true);
            EditorPrefs.SetString(EditorPrefsKey, json);
        }

        public static void ResetConfig()
        {
            _cachedConfig = null;
            EditorPrefs.DeleteKey(EditorPrefsKey);
        }

        public static int GetPort() => LoadConfig().port;

        public static void SetPort(int port)
        {
            var config = LoadConfig().Clone();
            config.port = port;
            SaveConfig(config);
        }

        public static bool GetAutoStart() => LoadConfig().autoStart;

        public static void SetAutoStart(bool autoStart)
        {
            var config = LoadConfig().Clone();
            config.autoStart = autoStart;
            SaveConfig(config);
        }

        public static bool GetLogRequests() => LoadConfig().logRequests;

        public static void SetLogRequests(bool logRequests)
        {
            var config = LoadConfig().Clone();
            config.logRequests = logRequests;
            SaveConfig(config);
        }

        #endregion

        #region UTCP Global Config (~/.utcp_config.json) - Raw JSON handling

        /// <summary>
        /// Read the raw JSON content of the global config file
        /// </summary>
        public static string ReadGlobalConfigRaw()
        {
            var path = GetUtcpConfigPath();
            if (File.Exists(path))
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[UTCP] Failed to read global config: {ex.Message}");
                }
            }
            return "{\"manual_call_templates\":[]}";
        }

        /// <summary>
        /// Write raw JSON content to the global config file
        /// </summary>
        public static void WriteGlobalConfigRaw(string json)
        {
            var path = GetUtcpConfigPath();
            try
            {
                File.WriteAllText(path, json);
                Debug.Log($"[UTCP] Saved global config to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UTCP] Failed to write global config: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse the global config as JObject
        /// </summary>
        private static JObject ParseGlobalConfig()
        {
            var json = ReadGlobalConfigRaw();
            try
            {
                return JObject.Parse(json);
            }
            catch
            {
                return new JObject { ["manual_call_templates"] = new JArray() };
            }
        }

        /// <summary>
        /// Get template info for display (extracts only name and type from raw JSON)
        /// </summary>
        public static TemplateDisplayInfo[] GetAllTemplatesForDisplay()
        {
            var config = ParseGlobalConfig();
            var templates = config["manual_call_templates"] as JArray ?? new JArray();

            return templates.Select(t => new TemplateDisplayInfo
            {
                rawJson = t.ToString(Formatting.None),
                name = t["name"]?.ToString() ?? "(unknown)",
                call_template_type = t["call_template_type"]?.ToString() ?? "(unknown)",
                url = t["url"]?.ToString() ?? ""
            }).ToArray();
        }

        /// <summary>
        /// Add a raw JSON template to the config
        /// </summary>
        public static bool AddTemplateRaw(string templateJson, out string error)
        {
            error = null;

            JObject template;
            try
            {
                template = JObject.Parse(templateJson);
            }
            catch (Exception ex)
            {
                error = $"Invalid JSON: {ex.Message}";
                return false;
            }

            var name = template["name"]?.ToString();
            var type = template["call_template_type"]?.ToString();

            if (string.IsNullOrEmpty(name))
            {
                error = "Template must have a 'name' field";
                return false;
            }

            if (string.IsNullOrEmpty(type))
            {
                error = "Template must have a 'call_template_type' field";
                return false;
            }

            var config = ParseGlobalConfig();
            var templates = config["manual_call_templates"] as JArray ?? new JArray();

            if (templates.Any(t => t["name"]?.ToString() == name))
            {
                error = $"Template '{name}' already exists";
                return false;
            }

            templates.Add(template);
            config["manual_call_templates"] = templates;
            WriteGlobalConfigRaw(config.ToString(Formatting.Indented));

            return true;
        }

        /// <summary>
        /// Remove a template by name
        /// </summary>
        public static bool RemoveTemplateByName(string name)
        {
            var config = ParseGlobalConfig();
            var templates = config["manual_call_templates"] as JArray ?? new JArray();

            var toRemove = templates.FirstOrDefault(t => t["name"]?.ToString() == name);
            if (toRemove == null)
                return false;

            templates.Remove(toRemove);
            config["manual_call_templates"] = templates;
            WriteGlobalConfigRaw(config.ToString(Formatting.Indented));
            return true;
        }

        /// <summary>
        /// Ensure the Unity Editor template exists in the global UTCP config with the correct port.
        /// Called automatically when the server starts.
        /// </summary>
        public static bool EnsureUnityTemplate(int port)
        {
            if (port <= 0)
            {
                Debug.LogWarning("[UTCP] Invalid port for template registration");
                return false;
            }

            var config = LoadConfig();
            var templateName = config.serverName;
            var expectedUrl = $"http://{config.address}:{port}/utcp";

            var templates = GetAllTemplatesForDisplay();
            var existing = templates.FirstOrDefault(t => t.name == templateName);
            
            var templateJson = $@"{{
  ""name"": ""{templateName}"",
  ""call_template_type"": ""http"",
  ""url"": ""{expectedUrl}"",
  ""http_method"": ""GET"",
  ""content_type"": ""application/json""
}}";

            if (existing == null)
            {
                // Create new template
                AddTemplateRaw(templateJson, out _);
                Debug.Log($"[UTCP] Created template '{templateName}' with port {port}");
                return true;
            }
            else if (existing.url != expectedUrl)
            {
                // Update existing template - remove and re-add with new URL
                RemoveTemplateByName(templateName);
                AddTemplateRaw(templateJson, out _);
                Debug.Log($"[UTCP] Updated template '{templateName}' port to {port}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the Unity template is registered in the global config
        /// </summary>
        public static bool IsTemplateRegistered(out string currentUrl)
        {
            var config = LoadConfig();
            var templateName = config.serverName;

            var templates = GetAllTemplatesForDisplay();
            var existing = templates.FirstOrDefault(t => t.name == templateName);

            currentUrl = existing?.url ?? "";
            return existing != null;
        }

        #endregion
    }

    /// <summary>
    /// Display info for a template (extracted from raw JSON)
    /// </summary>
    public class TemplateDisplayInfo
    {
        public string rawJson;
        public string name;
        public string call_template_type;
        public string url;
    }

}
