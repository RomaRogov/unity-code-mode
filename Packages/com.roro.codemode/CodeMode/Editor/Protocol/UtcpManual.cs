using System;
using System.Collections.Generic;

namespace CodeMode.Editor.Protocol
{
    /// <summary>
    /// Interface for the standard format for tool provider responses during discovery.
    /// Used to represent Unity Editor as a tool provider.
    /// </summary>
    [Serializable]
    public class UtcpManual
    {
        public string utcp_version;
        public string manual_version;
        public List<Tool> tools;

        public static UtcpManual Create(string manualVersion = "1.0.0")
        {
            return new UtcpManual
            {
                utcp_version = "1.0.1",
                manual_version = manualVersion,
                tools = new List<Tool>()
            };
        }
    }
}