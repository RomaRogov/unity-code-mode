using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Graphic))]
    public class GraphicAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Color;
        private SerializedProperty m_Material;
        private SerializedProperty m_RaycastTarget;
        private SerializedProperty m_RaycastPadding;

        protected override void OnEnable()
        {
            m_Color = serializedObject.FindProperty("m_Color");
            m_Material = serializedObject.FindProperty("m_Material");
            m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            m_RaycastPadding = serializedObject.FindProperty("m_RaycastPadding");

            AddSettingPropertyHandler("color",
                () => m_Color.colorValue.SerializeToJObject(),
                v => m_Color.colorValue = v.DeserializeToColor());

            AddSettingPropertyHandler("material",
                () => SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Material, v));

            AddSettingPropertyHandler("raycastTarget",
                () => new JValue(m_RaycastTarget.boolValue),
                v => m_RaycastTarget.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("raycastPadding",
                () => m_RaycastPadding.vector4Value.SerializeToJObject(),
                v => m_RaycastPadding.vector4Value = v.DeserializeToVector4());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("color", m_Color.colorValue.SerializeToJObject());
            DumpProperty("material", SerializeInstanceReferenceToJToken(m_Material.objectReferenceValue));
            DumpProperty("raycastTarget", m_RaycastTarget.boolValue);
            DumpProperty("raycastPadding", m_RaycastPadding.vector4Value.SerializeToJObject());
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("Graphic", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("color", "Color"),
                TsPropertyDef.Reference("material", "Material").Nullable(),
                TsPropertyDef.Field("raycastTarget", "boolean"),
                TsPropertyDef.Field("raycastPadding", "Vector4")
                    .WithComment("Padding (left, bottom, right, top) added to the raycast target"),
            });
        }
    }
}
