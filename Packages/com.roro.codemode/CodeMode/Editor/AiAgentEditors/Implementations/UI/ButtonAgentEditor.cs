using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    /// <summary>
    /// Dummy class for Button component
    /// TODO: Make UnityEvent inspection
    /// </summary>
    [CustomAiAgentEditor(typeof(Button))]
    public class ButtonAgentEditor : SelectableAgentEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            EmitClassDefinition("Button", new List<TsPropertyDef> { }, "Selectable");
        }
    }
}

