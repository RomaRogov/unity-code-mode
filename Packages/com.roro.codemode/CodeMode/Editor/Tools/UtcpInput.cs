namespace CodeMode.Editor.Tools
{
    /// <summary>
    /// Base class for UTCP tool input classes.
    /// Extend this class when you want to group multiple parameters into a single input object.
    /// If a tool method has a single parameter extending this class, schema will be generated from the class fields.
    /// Otherwise, schema will be generated from method parameters directly.
    /// </summary>
    public abstract class UtcpInput
    {
    }
}
