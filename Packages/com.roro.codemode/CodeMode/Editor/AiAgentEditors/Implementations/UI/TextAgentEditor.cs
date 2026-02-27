using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Text))]
    public class TextAgentEditor : GraphicAgentEditor
    {
        private SerializedProperty m_Text;
        private SerializedProperty m_FontData;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");

            AddSettingPropertyHandler("text",
                () => new JValue(m_Text.stringValue),
                v => m_Text.stringValue = v.Value<string>());

            AddSettingPropertyHandler("fontData",
                () => SerializeFontData(),
                v => DeserializeFontData(v));
        }

        private JObject SerializeFontData()
        {
            var font = m_FontData.FindPropertyRelative("m_Font");
            var fontSize = m_FontData.FindPropertyRelative("m_FontSize");
            var fontStyle = m_FontData.FindPropertyRelative("m_FontStyle");
            var bestFit = m_FontData.FindPropertyRelative("m_BestFit");
            var minSize = m_FontData.FindPropertyRelative("m_MinSize");
            var maxSize = m_FontData.FindPropertyRelative("m_MaxSize");
            var alignment = m_FontData.FindPropertyRelative("m_Alignment");
            var alignByGeometry = m_FontData.FindPropertyRelative("m_AlignByGeometry");
            var richText = m_FontData.FindPropertyRelative("m_RichText");
            var horizontalOverflow = m_FontData.FindPropertyRelative("m_HorizontalOverflow");
            var verticalOverflow = m_FontData.FindPropertyRelative("m_VerticalOverflow");
            var lineSpacing = m_FontData.FindPropertyRelative("m_LineSpacing");

            return new JObject
            {
                ["font"] = SerializeInstanceReferenceToJToken(font.objectReferenceValue),
                ["fontSize"] = fontSize.intValue,
                ["fontStyle"] = ((FontStyle)fontStyle.enumValueIndex).ToString(),
                ["bestFit"] = bestFit.boolValue,
                ["minSize"] = minSize.intValue,
                ["maxSize"] = maxSize.intValue,
                ["alignment"] = ((TextAnchor)alignment.enumValueIndex).ToString(),
                ["alignByGeometry"] = alignByGeometry.boolValue,
                ["richText"] = richText.boolValue,
                ["horizontalOverflow"] = ((HorizontalWrapMode)horizontalOverflow.enumValueIndex).ToString(),
                ["verticalOverflow"] = ((VerticalWrapMode)verticalOverflow.enumValueIndex).ToString(),
                ["lineSpacing"] = lineSpacing.floatValue
            };
        }

        private void DeserializeFontData(JToken v)
        {
            if (v is not JObject obj) return;

            var font = m_FontData.FindPropertyRelative("m_Font");
            var fontSize = m_FontData.FindPropertyRelative("m_FontSize");
            var fontStyle = m_FontData.FindPropertyRelative("m_FontStyle");
            var bestFit = m_FontData.FindPropertyRelative("m_BestFit");
            var minSize = m_FontData.FindPropertyRelative("m_MinSize");
            var maxSize = m_FontData.FindPropertyRelative("m_MaxSize");
            var alignment = m_FontData.FindPropertyRelative("m_Alignment");
            var alignByGeometry = m_FontData.FindPropertyRelative("m_AlignByGeometry");
            var richText = m_FontData.FindPropertyRelative("m_RichText");
            var horizontalOverflow = m_FontData.FindPropertyRelative("m_HorizontalOverflow");
            var verticalOverflow = m_FontData.FindPropertyRelative("m_VerticalOverflow");
            var lineSpacing = m_FontData.FindPropertyRelative("m_LineSpacing");

            if (obj.TryGetValue("font", out var f))
                SetObjectReferenceWithJTokenInstance(font, f);
            if (obj.TryGetValue("fontSize", out var fs))
                fontSize.intValue = fs.Value<int>();
            if (obj.TryGetValue("fontStyle", out var fst))
                fontStyle.enumValueIndex = (int)ParseEnum<FontStyle>(fst);
            if (obj.TryGetValue("bestFit", out var bf))
                bestFit.boolValue = bf.Value<bool>();
            if (obj.TryGetValue("minSize", out var mins))
                minSize.intValue = mins.Value<int>();
            if (obj.TryGetValue("maxSize", out var maxs))
                maxSize.intValue = maxs.Value<int>();
            if (obj.TryGetValue("alignment", out var al))
                alignment.enumValueIndex = (int)ParseEnum<TextAnchor>(al);
            if (obj.TryGetValue("alignByGeometry", out var abg))
                alignByGeometry.boolValue = abg.Value<bool>();
            if (obj.TryGetValue("richText", out var rt))
                richText.boolValue = rt.Value<bool>();
            if (obj.TryGetValue("horizontalOverflow", out var ho))
                horizontalOverflow.enumValueIndex = (int)ParseEnum<HorizontalWrapMode>(ho);
            if (obj.TryGetValue("verticalOverflow", out var vo))
                verticalOverflow.enumValueIndex = (int)ParseEnum<VerticalWrapMode>(vo);
            if (obj.TryGetValue("lineSpacing", out var ls))
                lineSpacing.floatValue = ls.Value<float>();
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();
            DumpProperty("text", m_Text.stringValue);
            DumpProperty("fontData", SerializeFontData());
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();
            GenerateEnumDefinition(typeof(FontStyle));
            GenerateEnumDefinition(typeof(TextAnchor));
            GenerateEnumDefinition(typeof(HorizontalWrapMode));
            GenerateEnumDefinition(typeof(VerticalWrapMode));

            EmitClassDefinition("FontData", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("font", "Font").Nullable(),
                TsPropertyDef.Field("fontSize", "number"),
                TsPropertyDef.Field("fontStyle", "FontStyle"),
                TsPropertyDef.Field("bestFit", "boolean"),
                TsPropertyDef.Field("minSize", "number"),
                TsPropertyDef.Field("maxSize", "number"),
                TsPropertyDef.Field("alignment", "TextAnchor"),
                TsPropertyDef.Field("alignByGeometry", "boolean"),
                TsPropertyDef.Field("richText", "boolean"),
                TsPropertyDef.Field("horizontalOverflow", "HorizontalWrapMode"),
                TsPropertyDef.Field("verticalOverflow", "VerticalWrapMode"),
                TsPropertyDef.Field("lineSpacing", "number"),
            });

            EmitClassDefinition("Text", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("text", "string"),
                TsPropertyDef.Field("fontData", "FontData"),
            }, "Graphic");
        }
    }
}


