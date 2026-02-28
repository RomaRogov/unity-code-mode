using System;
using System.Reflection;
using System.Threading.Tasks;
using CodeMode.Editor.Protocol;

namespace CodeMode.Editor.Tools.Registry
{
    /// <summary>
    /// Internal metadata about a registered tool
    /// </summary>
    public class ToolMetadata
    {
        public string Name;
        public string Description;
        public string HttpMethod;
        public string[] Tags;
        public MethodInfo Method;

        /// <summary>
        /// True if tool uses a single UtcpInput-derived class for input.
        /// False if tool uses direct method parameters.
        /// </summary>
        public bool UsesInputClass;

        /// <summary>
        /// Input class type (when UsesInputClass is true)
        /// </summary>
        public Type InputType;

        /// <summary>
        /// Method parameters info (when UsesInputClass is false)
        /// </summary>
        public ParameterInfo[] Parameters;

        public Type OutputType;
        public string BodyFieldName;
        public JsonSchema InputSchema;
        public JsonSchema OutputSchema;

        /// <summary>
        /// True when the tool returns void, Task, or non-generic UniTask.
        /// Fire-and-forget tools return immediately without waiting for execution.
        /// </summary>
        public bool IsFireAndForget;

        /// <summary>
        /// Pre-built delegate to await Task or UniTask results and box to object.
        /// Null for synchronous or fire-and-forget tools.
        /// </summary>
        public Func<object, Task<object>> AsyncAwaiter;

        /// <summary>
        /// Pre-built delegate to observe a fire-and-forget Task or UniTask for exceptions.
        /// Null for synchronous void tools.
        /// </summary>
        public Func<object, Task> FireAndForgetObserver;
    }
}
