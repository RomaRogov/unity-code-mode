using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(AspectRatioFitter))]
    public class AspectRatioFitterAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_AspectMode;
        private SerializedProperty m_AspectRatio;

        protected override void OnEnable()
        {
            m_AspectMode = serializedObject.FindProperty("m_AspectMode");
            m_AspectRatio = serializedObject.FindProperty("m_AspectRatio");

            AddSettingPropertyHandler("aspectMode",
                () => SerializeEnumToJValue(m_AspectMode),
                v => SetEnumValue(m_AspectMode, v));

            AddSettingPropertyHandler("aspectRatio",
                () => new JValue(m_AspectRatio.floatValue),
                v => m_AspectRatio.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("aspectMode", SerializeEnumToJValue(m_AspectMode));
            if (m_AspectMode.enumValueIndex != 0)
            {
                DumpProperty("aspectRatio", m_AspectRatio.floatValue);
            }
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(AspectRatioFitter.AspectMode));

            EmitClassDefinition("AspectRatioFitter", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("aspectMode", "AspectMode"),
                TsPropertyDef.Field("aspectRatio", "number"),
            });
        }
    }
}

