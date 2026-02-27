using System.Collections.Generic;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CodeMode.Editor.AiAgentEditors.Implementations.UI
{
    [CustomAiAgentEditor(typeof(Selectable))]
    public class SelectableAgentEditor : AiAgentEditor
    {
        private SerializedProperty m_Interactable;
        private SerializedProperty m_TargetGraphic;
        private SerializedProperty m_Transition;
        private SerializedProperty m_ColorBlock;
        private SerializedProperty m_SpriteState;
        private SerializedProperty m_AnimTrigger;
        private SerializedProperty m_Navigation;

        protected override void OnEnable()
        {
            m_Interactable = serializedObject.FindProperty("m_Interactable");
            m_TargetGraphic = serializedObject.FindProperty("m_TargetGraphic");
            m_Transition = serializedObject.FindProperty("m_Transition");
            m_ColorBlock = serializedObject.FindProperty("m_Colors");
            m_SpriteState = serializedObject.FindProperty("m_SpriteState");
            m_AnimTrigger = serializedObject.FindProperty("m_AnimationTriggers");
            m_Navigation = serializedObject.FindProperty("m_Navigation");

            AddSettingPropertyHandler("interactable",
                () => new JValue(m_Interactable.boolValue),
                v => m_Interactable.boolValue = v.Value<bool>());

            AddSettingPropertyHandler("targetGraphic",
                () => SerializeInstanceReferenceToJToken(m_TargetGraphic.objectReferenceValue),
                v => SetObjectReferenceWithJTokenInstance(m_TargetGraphic, v));

            AddSettingPropertyHandler("transition",
                () => SerializeEnumToJValue(m_Transition),
                v => SetEnumValue(m_Transition, v));

            AddSettingPropertyHandler("colors",
                () => SerializeColorBlock(),
                v => DeserializeColorBlock(v));

            AddSettingPropertyHandler("spriteState",
                () => SerializeSpriteState(),
                v => DeserializeSpriteState(v));

            AddSettingPropertyHandler("animationTriggers",
                () => SerializeAnimationTriggers(),
                v => DeserializeAnimationTriggers(v));

            AddSettingPropertyHandler("navigation",
                () => SerializeNavigation(),
                v => DeserializeNavigation(v));
        }

        private JObject SerializeColorBlock()
        {
            var normalColor = m_ColorBlock.FindPropertyRelative("m_NormalColor");
            var highlightedColor = m_ColorBlock.FindPropertyRelative("m_HighlightedColor");
            var pressedColor = m_ColorBlock.FindPropertyRelative("m_PressedColor");
            var selectedColor = m_ColorBlock.FindPropertyRelative("m_SelectedColor");
            var disabledColor = m_ColorBlock.FindPropertyRelative("m_DisabledColor");
            var colorMultiplier = m_ColorBlock.FindPropertyRelative("m_ColorMultiplier");
            var fadeDuration = m_ColorBlock.FindPropertyRelative("m_FadeDuration");

            return new JObject
            {
                ["normalColor"] = normalColor.colorValue.SerializeToJObject(),
                ["highlightedColor"] = highlightedColor.colorValue.SerializeToJObject(),
                ["pressedColor"] = pressedColor.colorValue.SerializeToJObject(),
                ["selectedColor"] = selectedColor.colorValue.SerializeToJObject(),
                ["disabledColor"] = disabledColor.colorValue.SerializeToJObject(),
                ["colorMultiplier"] = colorMultiplier.floatValue,
                ["fadeDuration"] = fadeDuration.floatValue
            };
        }

        private void DeserializeColorBlock(JToken v)
        {
            if (v is not JObject obj) return;

            var normalColor = m_ColorBlock.FindPropertyRelative("m_NormalColor");
            var highlightedColor = m_ColorBlock.FindPropertyRelative("m_HighlightedColor");
            var pressedColor = m_ColorBlock.FindPropertyRelative("m_PressedColor");
            var selectedColor = m_ColorBlock.FindPropertyRelative("m_SelectedColor");
            var disabledColor = m_ColorBlock.FindPropertyRelative("m_DisabledColor");
            var colorMultiplier = m_ColorBlock.FindPropertyRelative("m_ColorMultiplier");
            var fadeDuration = m_ColorBlock.FindPropertyRelative("m_FadeDuration");

            if (obj.TryGetValue("normalColor", out var nc))
                normalColor.colorValue = nc.DeserializeToColor();
            if (obj.TryGetValue("highlightedColor", out var hc))
                highlightedColor.colorValue = hc.DeserializeToColor();
            if (obj.TryGetValue("pressedColor", out var pc))
                pressedColor.colorValue = pc.DeserializeToColor();
            if (obj.TryGetValue("selectedColor", out var sc))
                selectedColor.colorValue = sc.DeserializeToColor();
            if (obj.TryGetValue("disabledColor", out var dc))
                disabledColor.colorValue = dc.DeserializeToColor();
            if (obj.TryGetValue("colorMultiplier", out var cm))
                colorMultiplier.floatValue = cm.Value<float>();
            if (obj.TryGetValue("fadeDuration", out var fd))
                fadeDuration.floatValue = fd.Value<float>();
        }

        private JObject SerializeSpriteState()
        {
            var highlightedSprite = m_SpriteState.FindPropertyRelative("m_HighlightedSprite");
            var pressedSprite = m_SpriteState.FindPropertyRelative("m_PressedSprite");
            var selectedSprite = m_SpriteState.FindPropertyRelative("m_SelectedSprite");
            var disabledSprite = m_SpriteState.FindPropertyRelative("m_DisabledSprite");

            return new JObject
            {
                ["highlightedSprite"] = SerializeInstanceReferenceToJToken(highlightedSprite.objectReferenceValue),
                ["pressedSprite"] = SerializeInstanceReferenceToJToken(pressedSprite.objectReferenceValue),
                ["selectedSprite"] = SerializeInstanceReferenceToJToken(selectedSprite.objectReferenceValue),
                ["disabledSprite"] = SerializeInstanceReferenceToJToken(disabledSprite.objectReferenceValue)
            };
        }

        private void DeserializeSpriteState(JToken v)
        {
            if (v is not JObject obj) return;

            var highlightedSprite = m_SpriteState.FindPropertyRelative("m_HighlightedSprite");
            var pressedSprite = m_SpriteState.FindPropertyRelative("m_PressedSprite");
            var selectedSprite = m_SpriteState.FindPropertyRelative("m_SelectedSprite");
            var disabledSprite = m_SpriteState.FindPropertyRelative("m_DisabledSprite");

            if (obj.TryGetValue("highlightedSprite", out var hs))
                SetObjectReferenceWithJTokenInstance(highlightedSprite, hs);

            if (obj.TryGetValue("pressedSprite", out var ps))
                SetObjectReferenceWithJTokenInstance(pressedSprite, ps);

            if (obj.TryGetValue("selectedSprite", out var ss))
                SetObjectReferenceWithJTokenInstance(selectedSprite, ss);

            if (obj.TryGetValue("disabledSprite", out var ds))
                SetObjectReferenceWithJTokenInstance(disabledSprite, ds);
        }

        private JObject SerializeAnimationTriggers()
        {
            var normalTrigger = m_AnimTrigger.FindPropertyRelative("m_NormalTrigger");
            var highlightedTrigger = m_AnimTrigger.FindPropertyRelative("m_HighlightedTrigger");
            var pressedTrigger = m_AnimTrigger.FindPropertyRelative("m_PressedTrigger");
            var selectedTrigger = m_AnimTrigger.FindPropertyRelative("m_SelectedTrigger");
            var disabledTrigger = m_AnimTrigger.FindPropertyRelative("m_DisabledTrigger");

            return new JObject
            {
                ["normalTrigger"] = normalTrigger.stringValue,
                ["highlightedTrigger"] = highlightedTrigger.stringValue,
                ["pressedTrigger"] = pressedTrigger.stringValue,
                ["selectedTrigger"] = selectedTrigger.stringValue,
                ["disabledTrigger"] = disabledTrigger.stringValue
            };
        }

        private void DeserializeAnimationTriggers(JToken v)
        {
            if (v is not JObject obj) return;

            var normalTrigger = m_AnimTrigger.FindPropertyRelative("m_NormalTrigger");
            var highlightedTrigger = m_AnimTrigger.FindPropertyRelative("m_HighlightedTrigger");
            var pressedTrigger = m_AnimTrigger.FindPropertyRelative("m_PressedTrigger");
            var selectedTrigger = m_AnimTrigger.FindPropertyRelative("m_SelectedTrigger");
            var disabledTrigger = m_AnimTrigger.FindPropertyRelative("m_DisabledTrigger");

            if (obj.TryGetValue("normalTrigger", out var nt))
                normalTrigger.stringValue = nt.Value<string>();
            if (obj.TryGetValue("highlightedTrigger", out var ht))
                highlightedTrigger.stringValue = ht.Value<string>();
            if (obj.TryGetValue("pressedTrigger", out var pt))
                pressedTrigger.stringValue = pt.Value<string>();
            if (obj.TryGetValue("selectedTrigger", out var st))
                selectedTrigger.stringValue = st.Value<string>();
            if (obj.TryGetValue("disabledTrigger", out var dt))
                disabledTrigger.stringValue = dt.Value<string>();
        }

        private JObject SerializeNavigation()
        {
            var mode = m_Navigation.FindPropertyRelative("m_Mode");
            var selectOnUp = m_Navigation.FindPropertyRelative("m_SelectOnUp");
            var selectOnDown = m_Navigation.FindPropertyRelative("m_SelectOnDown");
            var selectOnLeft = m_Navigation.FindPropertyRelative("m_SelectOnLeft");
            var selectOnRight = m_Navigation.FindPropertyRelative("m_SelectOnRight");

            return new JObject
            {
                ["mode"] = ((Navigation.Mode)mode.enumValueIndex).ToString(),
                ["selectOnUp"] = SerializeInstanceReferenceToJToken(selectOnUp.objectReferenceValue),
                ["selectOnDown"] = SerializeInstanceReferenceToJToken(selectOnDown.objectReferenceValue),
                ["selectOnLeft"] = SerializeInstanceReferenceToJToken(selectOnLeft.objectReferenceValue),
                ["selectOnRight"] = SerializeInstanceReferenceToJToken(selectOnRight.objectReferenceValue)
            };
        }

        private void DeserializeNavigation(JToken v)
        {
            if (v is not JObject obj) return;

            var mode = m_Navigation.FindPropertyRelative("m_Mode");
            var selectOnUp = m_Navigation.FindPropertyRelative("m_SelectOnUp");
            var selectOnDown = m_Navigation.FindPropertyRelative("m_SelectOnDown");
            var selectOnLeft = m_Navigation.FindPropertyRelative("m_SelectOnLeft");
            var selectOnRight = m_Navigation.FindPropertyRelative("m_SelectOnRight");

            if (obj.TryGetValue("mode", out var m))
                SetEnumValue(mode, m);

            if (obj.TryGetValue("selectOnUp", out var su))
                SetObjectReferenceWithJTokenInstance(selectOnUp, su);

            if (obj.TryGetValue("selectOnDown", out var sd))
                SetObjectReferenceWithJTokenInstance(selectOnDown, sd);

            if (obj.TryGetValue("selectOnLeft", out var sl))
                SetObjectReferenceWithJTokenInstance(selectOnLeft, sl);

            if (obj.TryGetValue("selectOnRight", out var sr))
                SetObjectReferenceWithJTokenInstance(selectOnRight, sr);
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("interactable", m_Interactable.boolValue);
            DumpProperty("targetGraphic", SerializeInstanceReferenceToJToken(m_TargetGraphic.objectReferenceValue));
            
            var transition = (Selectable.Transition)m_Transition.enumValueIndex;
            DumpProperty("transition", transition.ToString());

            if (transition == Selectable.Transition.ColorTint)
                DumpProperty("colors", SerializeColorBlock());
            else if (transition == Selectable.Transition.SpriteSwap)
                DumpProperty("spriteState", SerializeSpriteState());
            else if (transition == Selectable.Transition.Animation)
                DumpProperty("animationTriggers", SerializeAnimationTriggers());

            DumpProperty("navigation", SerializeNavigation());
        }

        protected override void OnDefinitionRequested()
        {
            GenerateEnumDefinition(typeof(Selectable.Transition));
            GenerateEnumDefinition(typeof(Navigation.Mode));

            EmitClassDefinition("ColorBlock", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("normalColor", "Color"),
                TsPropertyDef.Field("highlightedColor", "Color"),
                TsPropertyDef.Field("pressedColor", "Color"),
                TsPropertyDef.Field("selectedColor", "Color"),
                TsPropertyDef.Field("disabledColor", "Color"),
                TsPropertyDef.Field("colorMultiplier", "number"),
                TsPropertyDef.Field("fadeDuration", "number"),
            });

            EmitClassDefinition("SpriteState", new List<TsPropertyDef>
            {
                TsPropertyDef.Reference("highlightedSprite", "Sprite").Nullable(),
                TsPropertyDef.Reference("pressedSprite", "Sprite").Nullable(),
                TsPropertyDef.Reference("selectedSprite", "Sprite").Nullable(),
                TsPropertyDef.Reference("disabledSprite", "Sprite").Nullable(),
            });

            EmitClassDefinition("AnimationTriggers", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("normalTrigger", "string"),
                TsPropertyDef.Field("highlightedTrigger", "string"),
                TsPropertyDef.Field("pressedTrigger", "string"),
                TsPropertyDef.Field("selectedTrigger", "string"),
                TsPropertyDef.Field("disabledTrigger", "string"),
            });

            EmitClassDefinition("Navigation", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("mode", "Mode"),
                TsPropertyDef.Reference("selectOnUp", "Selectable").Nullable(),
                TsPropertyDef.Reference("selectOnDown", "Selectable").Nullable(),
                TsPropertyDef.Reference("selectOnLeft", "Selectable").Nullable(),
                TsPropertyDef.Reference("selectOnRight", "Selectable").Nullable(),
            });

            EmitClassDefinition("Selectable", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("interactable", "boolean"),
                TsPropertyDef.Reference("targetGraphic", "Graphic").Nullable(),
                TsPropertyDef.Field("transition", "Transition"),
                TsPropertyDef.Field("colors", "ColorBlock"),
                TsPropertyDef.Field("spriteState", "SpriteState"),
                TsPropertyDef.Field("animationTriggers", "AnimationTriggers"),
                TsPropertyDef.Field("navigation", "Navigation"),
            });
        }
    }
}

