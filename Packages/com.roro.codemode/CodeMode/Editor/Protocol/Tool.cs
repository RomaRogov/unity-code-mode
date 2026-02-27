using System;
using System.Collections.Generic;

namespace CodeMode.Editor.Protocol
{
    /// <summary>
    /// Represents a callable tool with its metadata, input/output schemas,
    /// and associated call template. Tools are the fundamental units of
    /// functionality in the UTCP ecosystem.
    /// </summary>
    [Serializable]
    public class Tool
    {
        public string name;
        public string description;
        public JsonSchema inputs;
        public JsonSchema outputs;
        public List<string> tags;
        public HttpCallTemplate tool_call_template;
    }
}