using System;
using CodeMode.Editor.Tools.Attributes;

namespace Editor
{
    public class HelloWorldTool
    {
        [Serializable]
        public class GreetOutput
        {
            public string message;
        }
        
        // Tool example: A simple tool that takes a name as input and returns a greeting message.
        // This example uses bodyField to receive the name through the HTTP body of a POST request.
        [UtcpTool("Returns a greeting message", httpMethod: "POST", bodyField: "name")]
        public static GreetOutput Greet(string name)
        {
            return new GreetOutput { message = $"Hello, {name}!" };
        }
    }
}