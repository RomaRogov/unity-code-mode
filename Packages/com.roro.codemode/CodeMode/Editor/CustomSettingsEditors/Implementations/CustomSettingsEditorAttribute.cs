using System;

namespace CodeMode.Editor.CustomSettingsEditors.Implementations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CustomSettingsEditorAttribute : Attribute
    {
        public string SettingsName { get; }
        public CustomSettingsEditorAttribute(string settingsName) => SettingsName = settingsName;
    }
}
