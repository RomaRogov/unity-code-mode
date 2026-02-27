using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(GameObject))]
    public class GameObjectAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Name;
        private SerializedProperty m_IsActive;
        private SerializedProperty m_StaticEditorFlags;
        private SerializedProperty m_Layer;
        private SerializedProperty m_TagString;

        protected override void OnEnable()
        {
            m_Name = serializedObject.FindProperty("m_Name");
            m_IsActive = serializedObject.FindProperty("m_IsActive");
            m_StaticEditorFlags = serializedObject.FindProperty("m_StaticEditorFlags");
            m_Layer = serializedObject.FindProperty("m_Layer");
            m_TagString = serializedObject.FindProperty("m_TagString");

            AddSettingPropertyHandler("name",
                () => new JValue(m_Name.stringValue),
                v => m_Name.stringValue = v.ToString());

            AddSettingPropertyHandler("active",
                () => new JValue(m_IsActive.boolValue),
                v => m_IsActive.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("isStatic",
                () => new JValue(m_StaticEditorFlags.intValue != 0),
                v =>
                {
                    var go = (GameObject)target;
                    var flags = v.Value<bool>() ? (StaticEditorFlags)(-1) : 0;
                    GameObjectUtility.SetStaticEditorFlags(go, flags);
                });

            AddSettingPropertyHandler("layer",
                () => new JValue(m_Layer.intValue),
                v =>
                {
                    if (v.Type == JTokenType.Integer)
                    {
                        m_Layer.intValue = v.Value<int>();
                        return;
                    }

                    var sanitizedName = v.ToString();
                    var layerIndex = LayerMask.NameToLayer(sanitizedName);
                    if (layerIndex == -1)
                    {
                        for (int i = 0; i < 32; i++)
                        {
                            var layerName = LayerMask.LayerToName(i);
                            if (!string.IsNullOrEmpty(layerName) &&
                                SanitizeIdentifier(layerName) == sanitizedName)
                            {
                                layerIndex = i;
                                break;
                            }
                        }
                    }

                    if (layerIndex == -1)
                        throw new Exception($"Invalid layer name: {sanitizedName}");
                    m_Layer.intValue = layerIndex;
                });

            AddSettingPropertyHandler("tag",
                () => new JValue(m_TagString.stringValue),
                v =>
                {
                    var tagNameSanitized = v.ToString();
                    if (Array.Exists(InternalEditorUtility.tags, t => t == tagNameSanitized))
                    {
                        m_TagString.stringValue = tagNameSanitized;
                        return;
                    }

                    var tagName = Array.Find(InternalEditorUtility.tags,
                        t => SanitizeIdentifier(t) == tagNameSanitized);
                    if (tagName != null)
                    {
                        m_TagString.stringValue = tagName;
                        return;
                    }

                    throw new Exception($"Invalid tag name: {tagNameSanitized}");
                });
            
            AddSettingPropertyHandler("transform",
                () =>
                {
                    var go = (GameObject)target;
                    return SerializeInstanceReferenceToJToken(go.transform);
                },
                v => throw new Exception("Transform reference is read-only"));
        }

        protected override void OnDumpRequested()
        {
            var go = (GameObject)target;
            DumpProperty("name", m_Name.stringValue);
            DumpProperty("active", go.activeInHierarchy);
            DumpProperty("transform", SerializeInstanceReferenceToJToken(go.transform));
            DumpProperty("layer", m_Layer.intValue);
            DumpProperty("activeSelf", m_IsActive.boolValue);
            DumpProperty("activeInHierarchy", go.activeInHierarchy);
            DumpProperty("isStatic", m_StaticEditorFlags.intValue != 0);
            DumpProperty("tag", m_TagString.stringValue);
        }

        protected override void OnDefinitionRequested()
        {
            var go = (GameObject)target;

            // Layer enum
            var layerEntries = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                    layerEntries.Add(new KeyValuePair<string, string>(SanitizeIdentifier(layerName), i.ToString()));
            }
            EmitCustomEnumDefinition("Layer", layerEntries);

            // Tag enum
            var tagEntries = new List<KeyValuePair<string, string>>();
            foreach (var t in InternalEditorUtility.tags)
            {
                if (!string.IsNullOrEmpty(t))
                    tagEntries.Add(new KeyValuePair<string, string>(SanitizeIdentifier(t), $"\"{t}\""));
            }
            EmitCustomEnumDefinition("Tag", tagEntries);

            // GameObject class
            var transformType = go.transform is RectTransform ? "RectTransform" : "Transform";
            EmitClassDefinition("GameObject", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("name", "string"),
                TsPropertyDef.Field("active", "boolean"),
                TsPropertyDef.Reference("transform", transformType).Readonly(),
                TsPropertyDef.Field("layer", "Layer"),
                TsPropertyDef.Field("activeSelf", "boolean").Readonly(),
                TsPropertyDef.Field("activeInHierarchy", "boolean").Readonly(),
                TsPropertyDef.Field("isStatic", "boolean"),
                TsPropertyDef.Field("tag", "Tag"),
            });
        }
    }
}
