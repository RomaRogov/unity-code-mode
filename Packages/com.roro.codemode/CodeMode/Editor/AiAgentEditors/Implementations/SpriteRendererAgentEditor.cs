using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(SpriteRenderer))]
    public class SpriteRendererAgentEditor : RendererAgentEditor
    {
        private SerializedProperty m_Sprite;
        private SerializedProperty m_Color;
        private SerializedProperty m_FlipX;
        private SerializedProperty m_FlipY;
        private SerializedProperty m_DrawMode;
        private SerializedProperty m_Size;
        private SerializedProperty m_SpriteTileMode;
        private SerializedProperty m_AdaptiveModeThreshold;
        private SerializedProperty m_MaskInteraction;
        private SerializedProperty m_SpriteSortPoint;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Color = serializedObject.FindProperty("m_Color");
            m_FlipX = serializedObject.FindProperty("m_FlipX");
            m_FlipY = serializedObject.FindProperty("m_FlipY");
            m_DrawMode = serializedObject.FindProperty("m_DrawMode");
            m_Size = serializedObject.FindProperty("m_Size");
            m_SpriteTileMode = serializedObject.FindProperty("m_SpriteTileMode");
            m_AdaptiveModeThreshold = serializedObject.FindProperty("m_AdaptiveModeThreshold");
            m_MaskInteraction = serializedObject.FindProperty("m_MaskInteraction");
            m_SpriteSortPoint = serializedObject.FindProperty("m_SpriteSortPoint");

            if (m_Sprite != null)
            {
                AddSettingPropertyHandler("sprite",
                    () => SerializeInstanceReferenceToJToken(m_Sprite.objectReferenceValue),
                    v => SetObjectReferenceWithJTokenInstance(m_Sprite, v));
            }

            if (m_Color != null)
            {
                AddSettingPropertyHandler("color",
                    () => m_Color.colorValue.SerializeToJObject(),
                    v => m_Color.colorValue = v.DeserializeToColor());
            }

            if (m_FlipX != null)
            {
                AddSettingPropertyHandler("flipX",
                    () => new JValue(m_FlipX.boolValue),
                    v => m_FlipX.boolValue = v.Value<bool>());
            }

            if (m_FlipY != null)
            {
                AddSettingPropertyHandler("flipY",
                    () => new JValue(m_FlipY.boolValue),
                    v => m_FlipY.boolValue = v.Value<bool>());
            }

            if (m_DrawMode != null)
            {
                AddSettingPropertyHandler("drawMode",
                    () => SerializePropertyValue(m_DrawMode),
                    v => SetEnumValue(m_DrawMode, v));
            }

            if (m_Size != null)
            {
                AddSettingPropertyHandler("size",
                    () => m_Size.vector2Value.SerializeToJObject(),
                    v => m_Size.vector2Value = v.DeserializeToVector2());
            }

            if (m_SpriteTileMode != null)
            {
                AddSettingPropertyHandler("tileMode",
                    () => SerializePropertyValue(m_SpriteTileMode),
                    v => SetEnumValue(m_SpriteTileMode, v));
            }

            if (m_AdaptiveModeThreshold != null)
            {
                AddSettingPropertyHandler("adaptiveModeThreshold",
                    () => new JValue(m_AdaptiveModeThreshold.floatValue),
                    v => m_AdaptiveModeThreshold.floatValue = v.Value<float>());
            }

            if (m_MaskInteraction != null)
            {
                AddSettingPropertyHandler("maskInteraction",
                    () => SerializePropertyValue(m_MaskInteraction),
                    v => SetEnumValue(m_MaskInteraction, v));
            }

            if (m_SpriteSortPoint != null)
            {
                AddSettingPropertyHandler("spriteSortPoint",
                    () => SerializePropertyValue(m_SpriteSortPoint),
                    v => SetEnumValue(m_SpriteSortPoint, v));
            }
        }

        protected override void OnDumpRequested()
        {
            base.OnDumpRequested();

            if (m_Sprite != null)
                DumpProperty("sprite", SerializeInstanceReferenceToJToken(m_Sprite.objectReferenceValue));

            if (m_Color != null)
                DumpProperty("color", m_Color.colorValue.SerializeToJObject());

            if (m_FlipX != null)
                DumpProperty("flipX", m_FlipX.boolValue);

            if (m_FlipY != null)
                DumpProperty("flipY", m_FlipY.boolValue);

            if (m_DrawMode != null)
            {
                var drawMode = (SpriteDrawMode)m_DrawMode.enumValueIndex;
                DumpProperty("drawMode", drawMode.ToString());

                // Size is only relevant for Sliced/Tiled modes
                if (drawMode != SpriteDrawMode.Simple && m_Size != null)
                    DumpProperty("size", m_Size.vector2Value.SerializeToJObject());

                // TileMode only relevant for Tiled mode
                if (drawMode == SpriteDrawMode.Tiled && m_SpriteTileMode != null)
                {
                    var tileMode = (SpriteTileMode)m_SpriteTileMode.enumValueIndex;
                    DumpProperty("tileMode", tileMode.ToString());

                    if (tileMode == SpriteTileMode.Adaptive && m_AdaptiveModeThreshold != null)
                        DumpProperty("adaptiveModeThreshold", m_AdaptiveModeThreshold.floatValue);
                }
            }

            if (m_MaskInteraction != null)
                DumpProperty("maskInteraction", ((SpriteMaskInteraction)m_MaskInteraction.enumValueIndex).ToString());

            if (m_SpriteSortPoint != null)
                DumpProperty("spriteSortPoint", ((SpriteSortPoint)m_SpriteSortPoint.enumValueIndex).ToString());
        }

        protected override void OnDefinitionRequested()
        {
            base.OnDefinitionRequested();

            GenerateEnumDefinition(typeof(SpriteDrawMode));
            GenerateEnumDefinition(typeof(SpriteTileMode));
            GenerateEnumDefinition(typeof(SpriteMaskInteraction));
            GenerateEnumDefinition(typeof(SpriteSortPoint));

            EmitClassDefinition("SpriteRenderer", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("sprite", "InstanceReference<Sprite>")
                    .WithComment("The Sprite to render"),
                TsPropertyDef.Field("color", "Color")
                    .WithComment("Rendering color for the Sprite"),
                TsPropertyDef.Field("flipX", "boolean")
                    .WithComment("Flip the sprite on the X axis"),
                TsPropertyDef.Field("flipY", "boolean")
                    .WithComment("Flip the sprite on the Y axis"),
                TsPropertyDef.Field("drawMode", "SpriteDrawMode")
                    .WithComment("Draw mode: Simple, Sliced, or Tiled"),
                TsPropertyDef.Field("size", "Vector2")
                    .WithComment("Sprite size (used in Sliced/Tiled draw modes)"),
                TsPropertyDef.Field("tileMode", "SpriteTileMode")
                    .WithComment("Tile mode when drawMode is Tiled: Continuous or Adaptive"),
                TsPropertyDef.Field("adaptiveModeThreshold", "number")
                    .WithComment("Threshold for adaptive tiling (0-1)"),
                TsPropertyDef.Field("maskInteraction", "SpriteMaskInteraction")
                    .WithComment("How the sprite interacts with SpriteMask"),
                TsPropertyDef.Field("spriteSortPoint", "SpriteSortPoint")
                    .WithComment("Sort point used for sprite distance calculation: Center or Pivot"),
            }, "Renderer");
        }
    }
}
