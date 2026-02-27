using System.Collections.Generic;
using CodeMode.Editor.AiAgentEditors;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CodeMode.Editor.CustomSettingsEditors.Implementations
{
    [CustomSettingsEditor("Physics")]
    public class PhysicsSettingsEditor : AiSettingsEditor
    {
        protected override void OnEnable()
        {
            AddSettingPropertyHandler("gravity",
                () => Physics.gravity.SerializeToJObject(),
                v => Physics.gravity = v.DeserializeToVector3());
            AddSettingPropertyHandler("bounceThreshold",
                () => new JValue(Physics.bounceThreshold),
                v => Physics.bounceThreshold = v.Value<float>());
            AddSettingPropertyHandler("defaultContactOffset",
                () => new JValue(Physics.defaultContactOffset),
                v => Physics.defaultContactOffset = v.Value<float>());
            AddSettingPropertyHandler("sleepThreshold",
                () => new JValue(Physics.sleepThreshold),
                v => Physics.sleepThreshold = v.Value<float>());
            AddSettingPropertyHandler("defaultSolverIterations",
                () => new JValue(Physics.defaultSolverIterations),
                v => Physics.defaultSolverIterations = v.Value<int>());
            AddSettingPropertyHandler("defaultSolverVelocityIterations",
                () => new JValue(Physics.defaultSolverVelocityIterations),
                v => Physics.defaultSolverVelocityIterations = v.Value<int>());
            AddSettingPropertyHandler("defaultMaxAngularSpeed",
                () => new JValue(Physics.defaultMaxAngularSpeed),
                v => Physics.defaultMaxAngularSpeed = v.Value<float>());
            AddSettingPropertyHandler("autoSyncTransforms",
                () => new JValue(Physics.autoSyncTransforms),
                v => Physics.autoSyncTransforms = v.Value<bool>());
            AddSettingPropertyHandler("reuseCollisionCallbacks",
                () => new JValue(Physics.reuseCollisionCallbacks),
                v => Physics.reuseCollisionCallbacks = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("gravity", Physics.gravity.SerializeToJObject());
            DumpProperty("bounceThreshold", Physics.bounceThreshold);
            DumpProperty("defaultContactOffset", Physics.defaultContactOffset);
            DumpProperty("sleepThreshold", Physics.sleepThreshold);
            DumpProperty("defaultSolverIterations", Physics.defaultSolverIterations);
            DumpProperty("defaultSolverVelocityIterations", Physics.defaultSolverVelocityIterations);
            DumpProperty("defaultMaxAngularSpeed", Physics.defaultMaxAngularSpeed);
            DumpProperty("autoSyncTransforms", Physics.autoSyncTransforms);
            DumpProperty("reuseCollisionCallbacks", Physics.reuseCollisionCallbacks);

            // Named layers
            var layers = new JArray();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                    layers.Add(new JObject { ["index"] = i, ["name"] = name });
            }
            DumpProperty("layers", layers);

            // Collision matrix
            var matrix = new JObject();
            for (int i = 0; i < 32; i++)
            {
                var nameA = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(nameA)) continue;

                var collidesWith = new JArray();
                for (int j = 0; j < 32; j++)
                {
                    var nameB = LayerMask.LayerToName(j);
                    if (string.IsNullOrEmpty(nameB)) continue;
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                        collidesWith.Add(nameB);
                }
                matrix[nameA] = collidesWith;
            }
            DumpProperty("collisionMatrix", matrix);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("PhysicsSettings", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("gravity", "Vector3"),
                TsPropertyDef.Field("bounceThreshold", "number"),
                TsPropertyDef.Field("defaultContactOffset", "number"),
                TsPropertyDef.Field("sleepThreshold", "number"),
                TsPropertyDef.Field("defaultSolverIterations", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("defaultSolverVelocityIterations", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("defaultMaxAngularSpeed", "number"),
                TsPropertyDef.Field("autoSyncTransforms", "boolean"),
                TsPropertyDef.Field("reuseCollisionCallbacks", "boolean"),
                TsPropertyDef.ArrayOf("layers", "PhysicsLayer").Readonly(),
                TsPropertyDef.Field("collisionMatrix", "Record<string, string[]>")
                    .WithComment("Per layer name → array of layer names it collides with"),
            });

            EmitClassDefinition("PhysicsLayer", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("index", "number").Readonly(),
                TsPropertyDef.Field("name", "string"),
            });
        }
    }
}
