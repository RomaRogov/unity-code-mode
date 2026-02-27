using System;

namespace CodeMode.Editor.Protocol
{
    /// <summary>
    /// Provider configuration for HTTP-based tools.
    /// Since we are acting as a server, this is the only type
    /// of call template we need to define, but UTCP protocol
    /// supports other types of call templates (e.g. mcp, cli)
    /// </summary>
    [Serializable]
    public class HttpCallTemplate
    {
        public string call_template_type = "http";
        public string http_method;
        public string request_body_format = "json";
        public string url;
        public string content_type = "application/json";
        public string body_field;

        public static HttpCallTemplate Get(string url)
        {
            return new HttpCallTemplate
            {
                http_method = "GET",
                url = url
            };
        }

        public static HttpCallTemplate Post(string url)
        {
            return new HttpCallTemplate
            {
                http_method = "POST",
                url = url
            };
        }
    }
}