using System;

namespace CodeMode.Editor.Config
{
    /// <summary>
    /// Unity Editor settings for the UTCP server (stored in EditorPrefs)
    /// </summary>
    [Serializable]
    public class UtcpConfig
    {
        public string address = "localhost";
        public int port = 0;  // 0 = auto-select free port
        public bool autoStart = true;
        public bool logRequests = false;
        public string serverName = "UnityEditor";
        public string utcpConfigPath = "";  // Path to ~/.utcp_config.json (empty = default)

        public UtcpConfig Clone()
        {
            return new UtcpConfig
            {
                address = address,
                port = port,
                autoStart = autoStart,
                logRequests = logRequests,
                serverName = serverName,
                utcpConfigPath = utcpConfigPath
            };
        }
    }
}
