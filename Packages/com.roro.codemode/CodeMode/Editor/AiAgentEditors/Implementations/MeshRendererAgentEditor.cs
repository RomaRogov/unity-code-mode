using System.Collections.Generic;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(MeshRenderer))]
    public class MeshRendererAgentEditor : RendererAgentEditor
    {
        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            EmitClassDefinition("MeshRenderer", new List<TsPropertyDef>(), "Renderer");
        }
    }
}
