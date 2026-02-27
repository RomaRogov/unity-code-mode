using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Shadow))]
    public class ShadowAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_EffectColor;
        private SerializedProperty m_EffectDistance;
        private SerializedProperty m_UseGraphicAlpha;

        protected override void OnEnable()
        {
            m_EffectColor = serializedObject.FindProperty("m_EffectColor");
            m_EffectDistance = serializedObject.FindProperty("m_EffectDistance");
            m_UseGraphicAlpha = serializedObject.FindProperty("m_UseGraphicAlpha");

            AddSettingPropertyHandler("effectColor",
                () => m_EffectColor.colorValue.SerializeToJObject(),
                v => m_EffectColor.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("effectDistance",
                () => m_EffectDistance.vector2Value.SerializeToJObject(),
                v => m_EffectDistance.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("useGraphicAlpha",
                () => new JValue(m_UseGraphicAlpha.boolValue),
                v => m_UseGraphicAlpha.boolValue = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("effectColor", m_EffectColor.colorValue.SerializeToJObject());
            DumpProperty("effectDistance", m_EffectDistance.vector2Value.SerializeToJObject());
            DumpProperty("useGraphicAlpha", m_UseGraphicAlpha.boolValue);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("Shadow", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("effectColor", "Color"),
                TsPropertyDef.Field("effectDistance", "Vector2"),
                TsPropertyDef.Field("useGraphicAlpha", "boolean"),
            });
        }
    }
}

