using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(RawImage))]
    public class RawImageAgentEditor : GraphicAgentEditor
    {
        private SerializedProperty m_Texture;
        private SerializedProperty m_UVRect;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_Texture = serializedObject.FindProperty("m_Texture");
            m_UVRect = serializedObject.FindProperty("m_UVRect");

            AddSettingPropertyHandler("texture",
                () => SerializeInstanceReferenceToJToken(m_Texture.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Texture, v));

            AddSettingPropertyHandler("uvRect",
                () => m_UVRect.rectValue.SerializeToJObject(),
                v => m_UVRect.rectValue = v.DeserializeToRect());
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            
            DumpProperty("texture", SerializeInstanceReferenceToJToken(m_Texture.objectReferenceValue));
            DumpProperty("uvRect", m_UVRect.rectValue.SerializeToJObject());
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            
            EmitClassDefinition("RawImage", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("texture", "Texture").Nullable(),
                TsPropertyDef.Field("uvRect", "Rect"),
            }, "Graphic");
        }
    }
}


