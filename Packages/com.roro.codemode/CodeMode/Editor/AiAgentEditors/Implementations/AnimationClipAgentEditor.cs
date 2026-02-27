using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(AnimationClip))]
    public class AnimationClipAgentEditor : AiAgentEditor
    {
        protected override void OnDumpRequested()
        {
            var clip = (AnimationClip)target;

            DumpProperty("length", clip.length);
            DumpProperty("frameRate", clip.frameRate);
            DumpProperty("wrapMode", clip.wrapMode.ToString());
            DumpProperty("isLooping", clip.isLooping);
            DumpProperty("legacy", clip.legacy);
            DumpProperty("humanMotion", clip.humanMotion);
            DumpProperty("empty", clip.empty);

            // Clip settings
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            DumpProperty("settings", new JObject
            {
                ["loopTime"] = settings.loopTime,
                ["loopBlend"] = settings.loopBlend,
                ["loopBlendOrientation"] = settings.loopBlendOrientation,
                ["loopBlendPositionY"] = settings.loopBlendPositionY,
                ["loopBlendPositionXZ"] = settings.loopBlendPositionXZ,
                ["keepOriginalOrientation"] = settings.keepOriginalOrientation,
                ["keepOriginalPositionY"] = settings.keepOriginalPositionY,
                ["keepOriginalPositionXZ"] = settings.keepOriginalPositionXZ,
                ["heightFromFeet"] = settings.heightFromFeet,
                ["mirror"] = settings.mirror,
                ["startTime"] = settings.startTime,
                ["stopTime"] = settings.stopTime,
                ["cycleOffset"] = settings.cycleOffset,
                ["hasAdditiveReferencePose"] = settings.hasAdditiveReferencePose
            });

            // Curve bindings
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var curveList = new JArray();
            foreach (var b in bindings)
            {
                var cd = new JObject
                {
                    ["path"] = b.path,
                    ["propertyName"] = b.propertyName,
                    ["type"] = b.type.Name
                };

                var curve = AnimationUtility.GetEditorCurve(clip, b);
                if (curve != null)
                    cd["keyframeCount"] = curve.keys.Length;

                curveList.Add(cd);
            }
            DumpProperty("curveBindings", curveList);

            // Object reference curves
            var objBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            var objCurveList = new JArray();
            foreach (var b in objBindings)
            {
                var cd = new JObject
                {
                    ["path"] = b.path,
                    ["propertyName"] = b.propertyName,
                    ["type"] = b.type.Name
                };

                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, b);
                if (keyframes != null)
                    cd["keyframeCount"] = keyframes.Length;

                objCurveList.Add(cd);
            }
            DumpProperty("objectReferenceCurves", objCurveList);

            // Events
            var events = AnimationUtility.GetAnimationEvents(clip);
            var eventList = new JArray();
            foreach (var e in events)
            {
                eventList.Add(new JObject
                {
                    ["time"] = e.time,
                    ["functionName"] = e.functionName,
                    ["stringParameter"] = e.stringParameter,
                    ["floatParameter"] = e.floatParameter,
                    ["intParameter"] = e.intParameter
                });
            }
            DumpProperty("events", eventList);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("AnimationClip", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("length", "number").Readonly(),
                TsPropertyDef.Field("frameRate", "number"),
                TsPropertyDef.Field("wrapMode", "string"),
                TsPropertyDef.Field("isLooping", "boolean").Readonly(),
                TsPropertyDef.Field("legacy", "boolean").Readonly(),
                TsPropertyDef.Field("humanMotion", "boolean").Readonly(),
                TsPropertyDef.Field("empty", "boolean").Readonly(),
                TsPropertyDef.Field("settings", "AnimationClipSettings"),
                TsPropertyDef.ArrayOf("curveBindings", "CurveBinding").Readonly(),
                TsPropertyDef.ArrayOf("objectReferenceCurves", "CurveBinding").Readonly(),
                TsPropertyDef.ArrayOf("events", "AnimationEvent"),
            });

            EmitClassDefinition("AnimationClipSettings", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("loopTime", "boolean"),
                TsPropertyDef.Field("loopBlend", "boolean"),
                TsPropertyDef.Field("loopBlendOrientation", "boolean"),
                TsPropertyDef.Field("loopBlendPositionY", "boolean"),
                TsPropertyDef.Field("loopBlendPositionXZ", "boolean"),
                TsPropertyDef.Field("keepOriginalOrientation", "boolean"),
                TsPropertyDef.Field("keepOriginalPositionY", "boolean"),
                TsPropertyDef.Field("keepOriginalPositionXZ", "boolean"),
                TsPropertyDef.Field("heightFromFeet", "boolean"),
                TsPropertyDef.Field("mirror", "boolean"),
                TsPropertyDef.Field("startTime", "number"),
                TsPropertyDef.Field("stopTime", "number"),
                TsPropertyDef.Field("cycleOffset", "number"),
                TsPropertyDef.Field("hasAdditiveReferencePose", "boolean"),
            });

            EmitClassDefinition("CurveBinding", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("path", "string").Readonly(),
                TsPropertyDef.Field("propertyName", "string").Readonly(),
                TsPropertyDef.Field("type", "string").Readonly(),
                TsPropertyDef.Field("keyframeCount", "number").Readonly(),
            });

            EmitClassDefinition("AnimationEvent", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("time", "number"),
                TsPropertyDef.Field("functionName", "string"),
                TsPropertyDef.Field("stringParameter", "string"),
                TsPropertyDef.Field("floatParameter", "number"),
                TsPropertyDef.Field("intParameter", "number"),
            });
        }
    }
}
