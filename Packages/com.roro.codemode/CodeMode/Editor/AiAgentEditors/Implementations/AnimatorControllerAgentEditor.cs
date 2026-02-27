using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace CodeMode.Editor.AiAgentEditors.Implementations
{
    [CustomAiAgentEditor(typeof(AnimatorController))]
    public class AnimatorControllerAgentEditor : AiAgentEditor
    {
        protected override void OnDumpRequested()
        {
            var ctrl = (AnimatorController)target;

            // Parameters
            var paramList = new JArray();
            foreach (var p in ctrl.parameters)
            {
                var pd = new JObject
                {
                    ["name"] = p.name,
                    ["type"] = p.type.ToString()
                };
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float:
                        pd["defaultValue"] = p.defaultFloat;
                        break;
                    case AnimatorControllerParameterType.Int:
                        pd["defaultValue"] = p.defaultInt;
                        break;
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Trigger:
                        pd["defaultValue"] = p.defaultBool;
                        break;
                }
                paramList.Add(pd);
            }
            DumpProperty("parameters", paramList);

            // Layers
            var layerList = new JArray();
            foreach (var layer in ctrl.layers)
            {
                var ld = new JObject
                {
                    ["name"] = layer.name,
                    ["blendingMode"] = layer.blendingMode.ToString(),
                    ["defaultWeight"] = layer.defaultWeight,
                    ["iKPass"] = layer.iKPass
                };

                if (layer.stateMachine != null)
                {
                    ld["defaultState"] = layer.stateMachine.defaultState?.name;
                    ld["states"] = DumpStates(layer.stateMachine);
                    ld["anyStateTransitions"] = DumpTransitions(layer.stateMachine.anyStateTransitions);
                }

                if (layer.avatarMask != null)
                    ld["avatarMask"] = layer.avatarMask.name;

                layerList.Add(ld);
            }
            DumpProperty("layers", layerList);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("AnimatorController", new List<TsPropertyDef>
            {
                TsPropertyDef.ArrayOf("parameters", "AnimatorParameter"),
                TsPropertyDef.ArrayOf("layers", "AnimatorLayer"),
            });

            EmitClassDefinition("AnimatorParameter", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("name", "string"),
                TsPropertyDef.Field("type", "\"Float\" | \"Int\" | \"Bool\" | \"Trigger\""),
                TsPropertyDef.Field("defaultValue", "number | boolean"),
            });

            EmitClassDefinition("AnimatorLayer", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("name", "string"),
                TsPropertyDef.Field("blendingMode", "\"Override\" | \"Additive\""),
                TsPropertyDef.Field("defaultWeight", "number"),
                TsPropertyDef.Field("iKPass", "boolean"),
                TsPropertyDef.Field("avatarMask", "string").Optional(),
                TsPropertyDef.Field("defaultState", "string"),
                TsPropertyDef.ArrayOf("states", "AnimatorState"),
                TsPropertyDef.ArrayOf("anyStateTransitions", "AnimatorTransition"),
            });

            EmitClassDefinition("AnimatorState", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("name", "string"),
                TsPropertyDef.Field("tag", "string"),
                TsPropertyDef.Field("speed", "number"),
                TsPropertyDef.Field("writeDefaultValues", "boolean"),
                TsPropertyDef.Reference("motion", "Motion").Nullable(),
                TsPropertyDef.ArrayOf("transitions", "AnimatorTransition"),
            });

            EmitClassDefinition("AnimatorTransition", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("destinationState", "string").Nullable(),
                TsPropertyDef.Field("hasExitTime", "boolean"),
                TsPropertyDef.Field("exitTime", "number"),
                TsPropertyDef.Field("duration", "number"),
                TsPropertyDef.Field("hasFixedDuration", "boolean"),
                TsPropertyDef.ArrayOf("conditions", "AnimatorCondition"),
            });

            EmitClassDefinition("AnimatorCondition", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("parameter", "string"),
                TsPropertyDef.Field("mode", "\"If\" | \"IfNot\" | \"Greater\" | \"Less\" | \"Equals\" | \"NotEqual\""),
                TsPropertyDef.Field("threshold", "number"),
            });
        }

        #region Dump Helpers

        private static JArray DumpStates(AnimatorStateMachine sm)
        {
            var states = new JArray();

            foreach (var child in sm.states)
            {
                var state = child.state;
                states.Add(new JObject
                {
                    ["name"] = state.name,
                    ["tag"] = state.tag,
                    ["speed"] = state.speed,
                    ["writeDefaultValues"] = state.writeDefaultValues,
                    ["motion"] = SerializeInstanceReferenceToJToken(state.motion),
                    ["transitions"] = DumpTransitions(state.transitions)
                });
            }

            foreach (var childSm in sm.stateMachines)
            {
                var subStates = DumpStates(childSm.stateMachine);
                foreach (JObject s in subStates)
                {
                    s["name"] = childSm.stateMachine.name + "/" + s["name"];
                    states.Add(s);
                }
            }

            return states;
        }

        private static JArray DumpTransitions(AnimatorStateTransition[] transitions)
        {
            var list = new JArray();
            foreach (var t in transitions)
            {
                list.Add(new JObject
                {
                    ["destinationState"] = t.destinationState?.name ?? t.destinationStateMachine?.name,
                    ["hasExitTime"] = t.hasExitTime,
                    ["exitTime"] = t.exitTime,
                    ["duration"] = t.duration,
                    ["hasFixedDuration"] = t.hasFixedDuration,
                    ["conditions"] = DumpConditions(t.conditions)
                });
            }
            return list;
        }

        private static JArray DumpConditions(AnimatorCondition[] conditions)
        {
            var list = new JArray();
            foreach (var c in conditions)
            {
                list.Add(new JObject
                {
                    ["parameter"] = c.parameter,
                    ["mode"] = c.mode.ToString(),
                    ["threshold"] = c.threshold
                });
            }
            return list;
        }

        #endregion
    }
}
