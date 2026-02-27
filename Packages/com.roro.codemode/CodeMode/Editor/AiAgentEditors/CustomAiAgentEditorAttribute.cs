using System;

namespace CodeMode.Editor.AiAgentEditors
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CustomAiAgentEditorAttribute : Attribute
    {
        public Type InspectedType { get; }
        public CustomAiAgentEditorAttribute(Type inspectedType) => InspectedType = inspectedType;
    }
}
