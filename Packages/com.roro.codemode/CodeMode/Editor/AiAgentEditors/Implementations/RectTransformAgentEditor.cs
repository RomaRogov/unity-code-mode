using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(RectTransform))]
    public class RectTransformAgentEditor : TransformAgentEditor
    {
        private SerializedProperty m_AnchoredPosition;
        private SerializedProperty m_SizeDelta;
        private SerializedProperty m_AnchorMin;
        private SerializedProperty m_AnchorMax;
        private SerializedProperty m_Pivot;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_AnchoredPosition = serializedObject.FindProperty("m_AnchoredPosition");
            m_SizeDelta = serializedObject.FindProperty("m_SizeDelta");
            m_AnchorMin = serializedObject.FindProperty("m_AnchorMin");
            m_AnchorMax = serializedObject.FindProperty("m_AnchorMax");
            m_Pivot = serializedObject.FindProperty("m_Pivot");

            AddSettingPropertyHandler("horizontal",
                () => GetLayoutAxisDump(0),
                v => ApplyLayoutAxis(0, v));
            AddSettingPropertyHandler("vertical",
                () => GetLayoutAxisDump(1),
                v => ApplyLayoutAxis(1, v));
            AddSettingPropertyHandler("pivot",
                () => m_Pivot.vector2Value.SerializeToJObject(),
                v => m_Pivot.vector2Value = v.DeserializeToVector2());
        }

        private RectTransform GetRectTransform() => (RectTransform)target;

        private Rect GetParentRect()
        {
            var rt = GetRectTransform();
            var parent = rt.parent as RectTransform;
            if (parent == null)
                return new Rect(0, 0, 0, 0);
            return parent.rect;
        }

        // Apply complete layout axis from JSON (handles mode switching)
        private void ApplyLayoutAxis(int axis, JToken data)
        {
            var mode = data["mode"]?.ToString();

            if (mode == "Point")
            {
                var anchor = data["anchor"]?.ToObject<float>() ?? 0.5f;
                var offset = data["offset"]?.ToObject<float>() ?? 0f;
                var size = data["size"]?.ToObject<float>() ?? 100f;

                // Convert to Point mode: set anchors equal, then apply offset and size
                ConvertToPointMode(axis, anchor, offset, size);
            }
            else if (mode == "Stretch")
            {
                var from = data["from"]?.ToObject<float>() ?? 0f;
                var to = data["to"]?.ToObject<float>() ?? 1f;
                var insetStart = data["insetStart"]?.ToObject<float>() ?? 0f;
                var insetEnd = data["insetEnd"]?.ToObject<float>() ?? 0f;

                // Convert to Stretch mode: set anchor range, then apply insets
                ConvertToStretchMode(axis, from, to, insetStart, insetEnd);
            }
        }

        // Convert axis to Point mode, maintaining visual position if possible
        private void ConvertToPointMode(int axis, float anchor, float offset, float size)
        {
            var rt = GetRectTransform();
            var anchorMin = m_AnchorMin.vector2Value;
            var anchorMax = m_AnchorMax.vector2Value;
            var wasStretched = !Mathf.Approximately(anchorMin[axis], anchorMax[axis]);

            if (wasStretched)
            {
                // When converting from Stretch to Point, calculate what anchor/offset would maintain position
                var offsetMin = rt.offsetMin;
                var offsetMax = rt.offsetMax;
                var parentRect = GetParentRect();
                var parentSize = axis == 0 ? parentRect.width : parentRect.height;

                // Calculate current center position in parent space
                var currentSize = offsetMax[axis] - offsetMin[axis];
                var currentCenter = anchorMin[axis] * parentSize + offsetMin[axis] + currentSize * 0.5f;

                // Calculate new offset to maintain visual position with new anchor
                offset = currentCenter - anchor * parentSize;
                size = currentSize;
            }

            // Set anchor (both min and max equal for Point mode)
            anchorMin[axis] = anchor;
            anchorMax[axis] = anchor;
            m_AnchorMin.vector2Value = anchorMin;
            m_AnchorMax.vector2Value = anchorMax;

            // Set offset (anchoredPosition)
            var pos = m_AnchoredPosition.vector2Value;
            pos[axis] = offset;
            m_AnchoredPosition.vector2Value = pos;

            // Set size (sizeDelta)
            var sizeDelta = m_SizeDelta.vector2Value;
            sizeDelta[axis] = size;
            m_SizeDelta.vector2Value = sizeDelta;
        }

        // Convert axis to Stretch mode, maintaining visual position if possible
        private void ConvertToStretchMode(int axis, float from, float to, float insetStart, float insetEnd)
        {
            var anchorMin = m_AnchorMin.vector2Value;
            var anchorMax = m_AnchorMax.vector2Value;
            var wasPoint = Mathf.Approximately(anchorMin[axis], anchorMax[axis]);

            if (wasPoint)
            {
                // When converting from Point to Stretch, calculate insets to maintain position/size
                var pos = m_AnchoredPosition.vector2Value;
                var sizeDelta = m_SizeDelta.vector2Value;
                var parentRect = GetParentRect();
                var parentSize = axis == 0 ? parentRect.width : parentRect.height;

                // Current position and size in parent space
                var currentAnchor = anchorMin[axis];
                var currentOffset = pos[axis];
                var currentSize = sizeDelta[axis];

                // Calculate position of left/bottom and right/top edges
                var edgeStart = currentAnchor * parentSize + currentOffset - currentSize * 0.5f;
                var edgeEnd = edgeStart + currentSize;

                // Calculate insets from new anchor positions
                insetStart = edgeStart - from * parentSize;
                insetEnd = to * parentSize - edgeEnd;
            }

            // Set anchor range
            anchorMin[axis] = from;
            anchorMax[axis] = to;
            m_AnchorMin.vector2Value = anchorMin;
            m_AnchorMax.vector2Value = anchorMax;

            // Compute sizeDelta and anchoredPosition from insets
            // newSd[axis] = (-insetEnd) - insetStart
            // anchoredPosition[axis] = insetStart + newSd[axis] * pivot[axis]
            var pivot = m_Pivot.vector2Value;
            var newSd = m_SizeDelta.vector2Value;
            var anchoredPos = m_AnchoredPosition.vector2Value;
            newSd[axis] = (-insetEnd) - insetStart;
            anchoredPos[axis] = insetStart + newSd[axis] * pivot[axis];
            m_SizeDelta.vector2Value = newSd;
            m_AnchoredPosition.vector2Value = anchoredPos;
        }

        private JObject GetLayoutAxisDump(int axis)
        {
            var rt = GetRectTransform();
            return CreateLayoutAxisObject(axis,
                m_AnchorMin.vector2Value, m_AnchorMax.vector2Value,
                rt.offsetMin, rt.offsetMax,
                m_AnchoredPosition.vector2Value, m_SizeDelta.vector2Value);
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("horizontal", GetLayoutAxisDump(0));
            DumpProperty("vertical", GetLayoutAxisDump(1));
            DumpProperty("pivot", m_Pivot.vector2Value.SerializeToJObject());
            base.OnDumpRequested();
        }

        private JObject CreateLayoutAxisObject(int axis, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Vector2 pos, Vector2 sizeDelta)
        {
            bool isStretched = !Mathf.Approximately(anchorMin[axis], anchorMax[axis]);

            if (isStretched)
            {
                // Stretch mode
                return new JObject
                {
                    ["mode"] = "Stretch",
                    ["from"] = anchorMin[axis],
                    ["to"] = anchorMax[axis],
                    ["insetStart"] = offsetMin[axis],
                    ["insetEnd"] = -offsetMax[axis] // Unity uses negative for max offset
                };
            }
            else
            {
                // Point mode
                return new JObject
                {
                    ["mode"] = "Point",
                    ["anchor"] = anchorMin[axis],
                    ["offset"] = pos[axis],
                    ["size"] = sizeDelta[axis]
                };
            }
        }

        protected override void OnDefinitionRequested()
        {
             base.OnDefinitionRequested();

            EmitClassDefinition("LayoutAxis", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("mode", "LayoutAxisMode").WithValue("'Point' | 'Stretch'")
            });

            EmitClassDefinition("LayoutAxisPoint", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("mode", "LayoutAxisMode").WithValue("'Point'"),
                TsPropertyDef.Field("anchor", "number").WithComment("Normalized anchor (0-1) relative to parent"),
                TsPropertyDef.Field("offset", "number").WithComment("Offset in pixels from the anchor point"),
                TsPropertyDef.Field("size", "number").WithComment("Size in pixels"),
            }, "LayoutAxis");

            EmitClassDefinition("LayoutAxisStretch", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("mode", "LayoutAxisMode").WithValue("'Stretch'"),
                TsPropertyDef.Field("from", "number").WithHeader("Normalized anchor borders (0-1)"),
                TsPropertyDef.Field("to", "number"),
                TsPropertyDef.Field("insetStart", "number").WithHeader("Margins in pixels"),
                TsPropertyDef.Field("insetEnd", "number")
            }, "LayoutAxis");

            EmitClassDefinition("RectTransform", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("horizontal", "LayoutAxis").WithComment("Horizontal layout mode and parameters"),
                TsPropertyDef.Field("vertical", "LayoutAxis").WithComment("Vertical layout mode and parameters"),
                TsPropertyDef.Field("pivot", "Vector2"),
            }, "Transform");
        }
    }
}
