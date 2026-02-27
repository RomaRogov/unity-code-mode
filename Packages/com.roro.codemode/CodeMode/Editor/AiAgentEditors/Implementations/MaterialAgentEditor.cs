using System;
using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(Material))]
    public class MaterialAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Shader;
        private SerializedProperty m_CustomRenderQueue;
        private SerializedProperty m_EnableInstancingVariants;
        private SerializedProperty m_DoubleSidedGI;
        private SerializedProperty m_LightmapFlags;
        private SerializedProperty m_ShaderKeywords;

        protected override void OnEnable()
        {
            var mat = (Material)target;

            m_Shader = serializedObject.FindProperty("m_Shader");
            m_CustomRenderQueue = serializedObject.FindProperty("m_CustomRenderQueue");
            m_EnableInstancingVariants = serializedObject.FindProperty("m_EnableInstancingVariants");
            m_DoubleSidedGI = serializedObject.FindProperty("m_DoubleSidedGI");
            m_LightmapFlags = serializedObject.FindProperty("m_LightmapFlags");
            m_ShaderKeywords = serializedObject.FindProperty("m_ShaderKeywords");

            if (m_Shader != null)
            {
                AddSettingPropertyHandler("shader",
                    () => m_Shader.objectReferenceValue != null
                        ? (JToken)((Shader)m_Shader.objectReferenceValue).name
                        : JValue.CreateNull(),
                    v =>
                    {
                        var shaderName = v.Value<string>();
                        var shader = Shader.Find(shaderName);
                        if (shader == null)
                            throw new Exception($"Shader not found: {shaderName}");
                        m_Shader.objectReferenceValue = shader;
                    });
            }

            if (m_CustomRenderQueue != null)
            {
                AddSettingPropertyHandler("renderQueue",
                    () => new JValue(m_CustomRenderQueue.intValue),
                    v => m_CustomRenderQueue.intValue = v.Value<int>());
            }

            if (m_EnableInstancingVariants != null)
            {
                AddSettingPropertyHandler("enableInstancing",
                    () => new JValue(m_EnableInstancingVariants.boolValue),
                    v => m_EnableInstancingVariants.boolValue = v.Value<bool>());
            }

            if (m_DoubleSidedGI != null)
            {
                AddSettingPropertyHandler("doubleSidedGI",
                    () => new JValue(m_DoubleSidedGI.boolValue),
                    v => m_DoubleSidedGI.boolValue = v.Value<bool>());
            }

            if (m_LightmapFlags != null)
            {
                AddSettingPropertyHandler("globalIlluminationFlags",
                    () => SerializeEnumToJValue(m_LightmapFlags),
                    v => SetEnumValue(m_LightmapFlags, v));
            }

            if (m_ShaderKeywords != null)
            {
                AddSettingPropertyHandler("shaderKeywords",
                    () => JToken.FromObject(m_ShaderKeywords.stringValue.Split(' ',
                        StringSplitOptions.RemoveEmptyEntries)),
                    v =>
                    {
                        var keywords = new List<string>();
                        foreach (var kw in (JArray)v)
                            keywords.Add(kw.Value<string>());
                        m_ShaderKeywords.stringValue = string.Join(" ", keywords);
                    });
            }

            if (!mat.shader) return;

            // Shader-specific properties use direct Material API (complex nested serialization)
            var shader = mat.shader;
            int count = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < count; i++)
            {
                var propName = ShaderUtil.GetPropertyName(shader, i);
                var propPath = AdaptMainPropertyName(propName);
                var propType = ShaderUtil.GetPropertyType(shader, i);

                switch (propType)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        AddSettingPropertyHandler(propPath,
                            () => mat.GetColor(propName).SerializeToJObject(),
                            v => mat.SetColor(propName, v.DeserializeToColor()));
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        AddSettingPropertyHandler(propPath,
                            () => mat.GetVector(propName).SerializeToJObject(),
                            v => mat.SetVector(propName, v.DeserializeToVector4()));
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                    case ShaderUtil.ShaderPropertyType.Range:
                        AddSettingPropertyHandler(propPath,
                            () => new JValue(mat.GetFloat(propName)),
                            v => mat.SetFloat(propName, v.Value<float>()));
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        AddSettingPropertyHandler(propPath, 
                            () => SerializeInstanceReferenceToJToken(mat.GetTexture(propName)),
                            v =>
                            {
                                if (v == null || v.Type == JTokenType.Null)
                                {
                                    mat.SetTexture(propName, null);
                                    return;
                                }
                                var id = v["id"];
                                if (id != null)
                                {
                                    mat.SetTexture(propName, (Texture)EditorUtility.InstanceIDToObject(id.Value<int>()));
                                }
                            });
                        AddSettingPropertyHandler($"{propPath}Scale",
                            () => mat.GetTextureScale(propName).SerializeToJObject(),
                            v => mat.SetTextureScale(propName, v.DeserializeToVector2()));
                        AddSettingPropertyHandler($"{propPath}Offset",
                            () => mat.GetTextureOffset(propName).SerializeToJObject(),
                            v => mat.SetTextureOffset(propName, v.DeserializeToVector2()));
                        break;
                }
            }
        }

        protected override void OnDumpRequested()
        {
            var mat = (Material)target;

            DumpProperty("shader", mat.shader ? (JToken)mat.shader.name : JValue.CreateNull());
            if (m_CustomRenderQueue != null)
                DumpProperty("renderQueue", m_CustomRenderQueue.intValue);
            if (m_EnableInstancingVariants != null)
                DumpProperty("enableInstancing", m_EnableInstancingVariants.boolValue);
            if (m_DoubleSidedGI != null)
                DumpProperty("doubleSidedGI", m_DoubleSidedGI.boolValue);
            if (m_LightmapFlags != null)
                DumpProperty("globalIlluminationFlags", SerializeEnumToJValue(m_LightmapFlags));
            if (m_ShaderKeywords != null)
                DumpProperty("shaderKeywords",
                    JToken.FromObject(m_ShaderKeywords.stringValue.Split(' ',
                        StringSplitOptions.RemoveEmptyEntries)));

            if (!mat.shader) return;

            var shader = mat.shader;
            int count = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < count; i++)
            {
                var propName = ShaderUtil.GetPropertyName(shader, i);
                var propPath = AdaptMainPropertyName(propName);
                var type = ShaderUtil.GetPropertyType(shader, i);

                switch (type)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        DumpProperty(propPath, mat.GetColor(propName).SerializeToJObject());
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        DumpProperty(propPath, mat.GetVector(propName).SerializeToJObject());
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                    case ShaderUtil.ShaderPropertyType.Range:
                        DumpProperty(propPath, mat.GetFloat(propName));
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        var tex = mat.GetTexture(propName);
                        DumpProperty(propPath, SerializeInstanceReferenceToJToken(tex));
                        DumpProperty($"{propPath}Scale", mat.GetTextureScale(propName).SerializeToJObject());
                        DumpProperty($"{propPath}Offset", mat.GetTextureOffset(propName).SerializeToJObject());
                        break;
                }
            }
        }

        protected override void OnDefinitionRequested()
        {
            var mat = (Material)target;

            GenerateEnumDefinition(typeof(MaterialGlobalIlluminationFlags));

            var fields = new List<TsPropertyDef>()
            {
                TsPropertyDef.Field("shader", "string").WithComment("Shader name (e.g. \"Standard\", \"Unlit/Texture\")"),
                TsPropertyDef.Field("renderQueue", "number")
                    .WithComment("Render queue value; -1 means use value from shader"),
                TsPropertyDef.Field("enableInstancing", "boolean"),
                TsPropertyDef.Field("doubleSidedGI", "boolean"),
                TsPropertyDef.Field("globalIlluminationFlags", "MaterialGlobalIlluminationFlags"),
                TsPropertyDef.ArrayOf("shaderKeywords", "string"),
            };

            if (mat.shader)
            {
                var shader = mat.shader;
                int count = ShaderUtil.GetPropertyCount(shader);

                for (int i = 0; i < count; i++)
                {
                    var name = AdaptMainPropertyName(ShaderUtil.GetPropertyName(shader, i));
                    var type = ShaderUtil.GetPropertyType(shader, i);
                    var desc = ShaderUtil.GetPropertyDescription(shader, i);

                    TsPropertyDef prop = null;
                    switch (type)
                    {
                        case ShaderUtil.ShaderPropertyType.Color:
                            prop = TsPropertyDef.Field(name, "Color");
                            break;
                        case ShaderUtil.ShaderPropertyType.Vector:
                            prop = TsPropertyDef.Field(name, "Vector4");
                            break;
                        case ShaderUtil.ShaderPropertyType.Float:
                            prop = TsPropertyDef.Field(name, "number");
                            break;
                        case ShaderUtil.ShaderPropertyType.Range:
                            var min = ShaderUtil.GetRangeLimits(shader, i, 1);
                            var max = ShaderUtil.GetRangeLimits(shader, i, 2);
                            prop = TsPropertyDef.Field(name, "number")
                                .WithDecorator($"min: {min}, max: {max}");
                            break;
                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            var texProp = TsPropertyDef.Reference(name, "Texture").Nullable();
                            if (!string.IsNullOrEmpty(desc))
                                texProp.WithComment(desc);
                            fields.Add(texProp);
                            fields.Add(TsPropertyDef.Field($"{name}Scale", "Vector2"));
                            fields.Add(TsPropertyDef.Field($"{name}Offset", "Vector2"));
                            break;
                    }

                    if (prop != null)
                    {
                        if (i == 0)
                            prop.WithHeader($"Shader: {shader.name}");
                        if (!string.IsNullOrEmpty(desc))
                            prop.WithComment(desc);
                        fields.Add(prop);
                    }
                }
            }

            EmitClassDefinition("Material", fields);
        }
        
        private static string AdaptMainPropertyName(string name)
        {
            if (name == "_Color")
                return "color";
            if (name == "_MainTex")
                return "mainTexture";
            return name;
        }
    }
}
