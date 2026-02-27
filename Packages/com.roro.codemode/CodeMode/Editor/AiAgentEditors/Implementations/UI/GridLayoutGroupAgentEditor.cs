using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(GridLayoutGroup))]
    public class GridLayoutGroupAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Padding;
        private SerializedProperty m_CellSize;
        private SerializedProperty m_Spacing;
        private SerializedProperty m_StartCorner;
        private SerializedProperty m_StartAxis;
        private SerializedProperty m_ChildAlignment;
        private SerializedProperty m_Constraint;
        private SerializedProperty m_ConstraintCount;

        protected override void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_CellSize = serializedObject.FindProperty("m_CellSize");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_StartCorner = serializedObject.FindProperty("m_StartCorner");
            m_StartAxis = serializedObject.FindProperty("m_StartAxis");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_Constraint = serializedObject.FindProperty("m_Constraint");
            m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");

            AddSettingPropertyHandler("padding",
                () => GetPaddingDump(),
                v => SetPadding(v));

            AddSettingPropertyHandler("cellSize",
                () => m_CellSize.vector2Value.SerializeToJObject(),
                v => m_CellSize.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("spacing",
                () => m_Spacing.vector2Value.SerializeToJObject(),
                v => m_Spacing.vector2Value = v.DeserializeToVector2());

            AddSettingPropertyHandler("startCorner",
                () => SerializeEnumToJValue(m_StartCorner),
                v => SetEnumValue(m_StartCorner, v));

            AddSettingPropertyHandler("startAxis",
                () => SerializeEnumToJValue(m_StartAxis),
                v => SetEnumValue(m_StartAxis, v));

            AddSettingPropertyHandler("childAlignment",
                () => SerializeEnumToJValue(m_ChildAlignment),
                v => SetEnumValue(m_ChildAlignment, v));

            AddSettingPropertyHandler("constraint",
                () => SerializeEnumToJValue(m_Constraint),
                v => SetEnumValue(m_ConstraintCount, v));

            AddSettingPropertyHandler("constraintCount",
                () => new JValue(m_ConstraintCount.intValue),
                v => m_ConstraintCount.intValue = v.Value<int>());
        }

        private JObject GetPaddingDump()
        {
            return new JObject
            {
                ["left"] = m_Padding.FindPropertyRelative("m_Left").intValue,
                ["right"] = m_Padding.FindPropertyRelative("m_Right").intValue,
                ["top"] = m_Padding.FindPropertyRelative("m_Top").intValue,
                ["bottom"] = m_Padding.FindPropertyRelative("m_Bottom").intValue
            };
        }

        private void SetPadding(JToken v)
        {
            if (v is JObject obj)
            {
                if (obj.TryGetValue("left", out var left))
                    m_Padding.FindPropertyRelative("m_Left").intValue = left.Value<int>();
                if (obj.TryGetValue("right", out var right))
                    m_Padding.FindPropertyRelative("m_Right").intValue = right.Value<int>();
                if (obj.TryGetValue("top", out var top))
                    m_Padding.FindPropertyRelative("m_Top").intValue = top.Value<int>();
                if (obj.TryGetValue("bottom", out var bottom))
                    m_Padding.FindPropertyRelative("m_Bottom").intValue = bottom.Value<int>();
            }
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("padding", GetPaddingDump());
            DumpProperty("cellSize", m_CellSize.vector2Value.SerializeToJObject());
            DumpProperty("spacing", m_Spacing.vector2Value.SerializeToJObject());
            DumpProperty("startCorner", SerializeEnumToJValue(m_StartCorner));
            DumpProperty("startAxis", SerializeEnumToJValue(m_StartAxis));
            DumpProperty("childAlignment", SerializeEnumToJValue(m_ChildAlignment));
            DumpProperty("constraint", SerializeEnumToJValue(m_Constraint));
            DumpProperty("constraintCount", m_ConstraintCount.intValue);
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(TextAnchor));
            GenerateEnumDefinition(typeof(GridLayoutGroup.Corner));
            GenerateEnumDefinition(typeof(GridLayoutGroup.Axis));
            GenerateEnumDefinition(typeof(GridLayoutGroup.Constraint));

            EmitClassDefinition("RectOffsetPadding", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("left", "number"),
                TsPropertyDef.Field("right", "number"),
                TsPropertyDef.Field("top", "number"),
                TsPropertyDef.Field("bottom", "number"),
            });

            EmitClassDefinition("GridLayoutGroup", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("padding", "RectOffsetPadding"),
                TsPropertyDef.Field("cellSize", "Vector2"),
                TsPropertyDef.Field("spacing", "Vector2"),
                TsPropertyDef.Field("startCorner", "Corner"),
                TsPropertyDef.Field("startAxis", "Axis"),
                TsPropertyDef.Field("childAlignment", "TextAnchor"),
                TsPropertyDef.Field("constraint", "Constraint"),
                TsPropertyDef.Field("constraintCount", "number")
                    .WithComment("Only used when constraint is FixedColumnCount or FixedRowCount"),
            });
        }
    }
}
