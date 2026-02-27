using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(GraphicRaycaster))]
    public class GraphicRaycasterAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_IgnoreReversedGraphics;
        private SerializedProperty m_BlockingObjects;
        private SerializedProperty m_BlockingMask;

        protected override void OnEnable()
        {
            m_IgnoreReversedGraphics = serializedObject.FindProperty("m_IgnoreReversedGraphics");
            m_BlockingObjects = serializedObject.FindProperty("m_BlockingObjects");
            m_BlockingMask = serializedObject.FindProperty("m_BlockingMask");

            AddSettingPropertyHandler("ignoreReversedGraphics",
                () => new JValue(m_IgnoreReversedGraphics.boolValue),
                v => m_IgnoreReversedGraphics.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("blockingObjects",
                () => SerializeEnumToJValue(m_BlockingObjects),
                v => SetEnumValue(m_BlockingObjects, v));

            AddSettingPropertyHandler("blockingMask",
                () => new JValue(m_BlockingMask.intValue),
                v => m_BlockingMask.intValue = v.Value<int>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("ignoreReversedGraphics", m_IgnoreReversedGraphics.boolValue);
            DumpProperty("blockingObjects", SerializeEnumToJValue(m_BlockingObjects));
            
            // Only dump blocking mask if blocking objects is not None
            var blockingObjects = (GraphicRaycaster.BlockingObjects)m_BlockingObjects.enumValueIndex;
            if (blockingObjects != GraphicRaycaster.BlockingObjects.None)
            {
                DumpProperty("blockingMask", m_BlockingMask.intValue);
            }
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(GraphicRaycaster.BlockingObjects));

            EmitClassDefinition("GraphicRaycaster", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("ignoreReversedGraphics", "boolean")
                    .WithComment("Should graphics facing away from the raycaster be considered?"),
                
                TsPropertyDef.Field("blockingObjects", "BlockingObjects")
                    .WithComment("Type of 3D objects that will block raycasts (None, TwoD, ThreeD, or All)"),
                
                TsPropertyDef.Field("blockingMask", "number")
                    .WithComment("Layer mask for 3D objects that can block raycasts (only used when blockingObjects is not None)"),
            });
        }
    }
}


