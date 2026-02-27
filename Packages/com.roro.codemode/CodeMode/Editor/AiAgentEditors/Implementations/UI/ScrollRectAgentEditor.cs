using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(ScrollRect))]
    public class ScrollRectAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Content;
        private SerializedProperty m_Horizontal;
        private SerializedProperty m_Vertical;
        private SerializedProperty m_MovementType;
        private SerializedProperty m_Elasticity;
        private SerializedProperty m_Inertia;
        private SerializedProperty m_DecelerationRate;
        private SerializedProperty m_ScrollSensitivity;
        private SerializedProperty m_Viewport;
        private SerializedProperty m_HorizontalScrollbar;
        private SerializedProperty m_VerticalScrollbar;
        private SerializedProperty m_HorizontalScrollbarVisibility;
        private SerializedProperty m_VerticalScrollbarVisibility;
        private SerializedProperty m_HorizontalScrollbarSpacing;
        private SerializedProperty m_VerticalScrollbarSpacing;

        protected override void OnEnable()
        {
            m_Content = serializedObject.FindProperty("m_Content");
            m_Horizontal = serializedObject.FindProperty("m_Horizontal");
            m_Vertical = serializedObject.FindProperty("m_Vertical");
            m_MovementType = serializedObject.FindProperty("m_MovementType");
            m_Elasticity = serializedObject.FindProperty("m_Elasticity");
            m_Inertia = serializedObject.FindProperty("m_Inertia");
            m_DecelerationRate = serializedObject.FindProperty("m_DecelerationRate");
            m_ScrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
            m_Viewport = serializedObject.FindProperty("m_Viewport");
            m_HorizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
            m_VerticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");
            m_HorizontalScrollbarVisibility = serializedObject.FindProperty("m_HorizontalScrollbarVisibility");
            m_VerticalScrollbarVisibility = serializedObject.FindProperty("m_VerticalScrollbarVisibility");
            m_HorizontalScrollbarSpacing = serializedObject.FindProperty("m_HorizontalScrollbarSpacing");
            m_VerticalScrollbarSpacing = serializedObject.FindProperty("m_VerticalScrollbarSpacing");

            AddSettingPropertyHandler("content",
                () => SerializeInstanceReferenceToJToken(m_Content.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Content, v));

            AddSettingPropertyHandler("horizontal",
                () => new JValue(m_Horizontal.boolValue),
                v => m_Horizontal.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("vertical",
                () => new JValue(m_Vertical.boolValue),
                v => m_Vertical.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("movementType",
                () => SerializeEnumToJValue(m_MovementType),
                v => SetEnumValue(m_MovementType, v));

            AddSettingPropertyHandler("elasticity",
                () => new JValue(m_Elasticity.floatValue),
                v => m_Elasticity.floatValue = v.Value<float>());

            AddSettingPropertyHandler("inertia",
                () => new JValue(m_Inertia.boolValue),
                v => m_Inertia.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("decelerationRate",
                () => new JValue(m_DecelerationRate.floatValue),
                v => m_DecelerationRate.floatValue = v.Value<float>());

            AddSettingPropertyHandler("scrollSensitivity",
                () => new JValue(m_ScrollSensitivity.floatValue),
                v => m_ScrollSensitivity.floatValue = v.Value<float>());

            AddSettingPropertyHandler("viewport",
                () => SerializeInstanceReferenceToJToken(m_Viewport.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_Viewport, v));

            AddSettingPropertyHandler("horizontalScrollbar",
                () => SerializeInstanceReferenceToJToken(m_HorizontalScrollbar.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_HorizontalScrollbar, v));

            AddSettingPropertyHandler("verticalScrollbar",
                () => SerializeInstanceReferenceToJToken(m_VerticalScrollbar.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_VerticalScrollbar, v));

            AddSettingPropertyHandler("horizontalScrollbarVisibility",
                () => SerializeEnumToJValue(m_HorizontalScrollbarVisibility),
                v => SetEnumValue(m_HorizontalScrollbarVisibility, v));

            AddSettingPropertyHandler("verticalScrollbarVisibility",
                () => SerializeEnumToJValue(m_VerticalScrollbarVisibility),
                v => SetEnumValue(m_VerticalScrollbarVisibility, v));

            AddSettingPropertyHandler("horizontalScrollbarSpacing",
                () => new JValue(m_HorizontalScrollbarSpacing.floatValue),
                v => m_HorizontalScrollbarSpacing.floatValue = v.Value<float>());

            AddSettingPropertyHandler("verticalScrollbarSpacing",
                () => new JValue(m_VerticalScrollbarSpacing.floatValue),
                v => m_VerticalScrollbarSpacing.floatValue = v.Value<float>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("content", SerializeInstanceReferenceToJToken(m_Content.objectReferenceValue));
            DumpProperty("horizontal", m_Horizontal.boolValue);
            DumpProperty("vertical", m_Vertical.boolValue);
            DumpProperty("movementType", SerializeEnumToJValue(m_MovementType));
            if (m_MovementType.enumValueIndex == (int)ScrollRect.MovementType.Elastic)
                DumpProperty("elasticity", m_Elasticity.floatValue);
            DumpProperty("inertia", m_Inertia.boolValue);
            if (m_Inertia.boolValue)
                DumpProperty("decelerationRate", m_DecelerationRate.floatValue);
            DumpProperty("scrollSensitivity", m_ScrollSensitivity.floatValue);
            DumpProperty("viewport", SerializeInstanceReferenceToJToken(m_Viewport.objectReferenceValue));
            DumpProperty("horizontalScrollbar", SerializeInstanceReferenceToJToken(m_HorizontalScrollbar.objectReferenceValue));
            DumpProperty("verticalScrollbar", SerializeInstanceReferenceToJToken(m_VerticalScrollbar.objectReferenceValue));
            DumpProperty("horizontalScrollbarVisibility", SerializeEnumToJValue(m_HorizontalScrollbarVisibility));
            DumpProperty("verticalScrollbarVisibility", SerializeEnumToJValue(m_VerticalScrollbarVisibility));
            DumpProperty("horizontalScrollbarSpacing", m_HorizontalScrollbarSpacing.floatValue);
            DumpProperty("verticalScrollbarSpacing", m_VerticalScrollbarSpacing.floatValue);
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(ScrollRect.MovementType));
            GenerateEnumDefinition(typeof(ScrollRect.ScrollbarVisibility));

            EmitClassDefinition("ScrollRect", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("content", "RectTransform").Nullable(),
                TsPropertyDef.Field("horizontal", "boolean"),
                TsPropertyDef.Field("vertical", "boolean"),
                TsPropertyDef.Field("movementType", "MovementType"),
                TsPropertyDef.Field("elasticity", "number"),
                TsPropertyDef.Field("inertia", "boolean"),
                TsPropertyDef.Field("decelerationRate", "number"),
                TsPropertyDef.Field("scrollSensitivity", "number"),
                TsPropertyDef.Reference("viewport", "RectTransform").Nullable(),
                TsPropertyDef.Reference("horizontalScrollbar", "Scrollbar").Nullable(),
                TsPropertyDef.Reference("verticalScrollbar", "Scrollbar").Nullable(),
                TsPropertyDef.Field("horizontalScrollbarVisibility", "ScrollbarVisibility"),
                TsPropertyDef.Field("verticalScrollbarVisibility", "ScrollbarVisibility"),
                TsPropertyDef.Field("horizontalScrollbarSpacing", "number"),
                TsPropertyDef.Field("verticalScrollbarSpacing", "number"),
            });
        }
    }
}


