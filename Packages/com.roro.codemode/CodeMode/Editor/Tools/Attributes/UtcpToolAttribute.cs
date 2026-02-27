using System;

namespace CodeMode.Editor.Tools.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UtcpToolAttribute : Attribute
    {
        public readonly string description;
        public readonly string httpMethod; 
        public readonly string[] tags;
        public readonly string bodyField;

        /// <summary>
        /// Marks a method as a UTCP tool callable via HTTP API.
        /// </summary>
        /// <param name="description">Brief description of what the tool does</param>
        /// <param name="httpMethod">HTTP method to use for this tool (GET, POST, etc.)</param>
        /// <param name="tags">Tags for tool search and categorization</param>
        /// <param name="bodyField">Name of field to receive through HTTP body (POST, PUT methods)</param>
        public UtcpToolAttribute(string description, string[] tags = null, string httpMethod = "GET", string bodyField = null)
        {
            this.description = description;
            this.httpMethod = httpMethod;
            this.tags = tags ?? Array.Empty<string>();
            this.bodyField = bodyField;
        }
    }

}
