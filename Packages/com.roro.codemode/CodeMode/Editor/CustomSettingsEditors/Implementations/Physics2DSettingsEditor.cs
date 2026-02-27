using System.Collections.Generic;
using CodeMode.Editor.AiAgentEditors;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CodeMode.Editor.CustomSettingsEditors.Implementations
{
    [CustomSettingsEditor("Physics2D")]
    public class Physics2DSettingsEditor : AiSettingsEditor
    {
        protected override void OnEnable()
        {
            AddSettingPropertyHandler("gravity",
                () => Physics2D.gravity.SerializeToJObject(),
                v => Physics2D.gravity = v.DeserializeToVector2());
            AddSettingPropertyHandler("defaultContactOffset",
                () => new JValue(Physics2D.defaultContactOffset),
                v => Physics2D.defaultContactOffset = v.Value<float>());
            AddSettingPropertyHandler("velocityIterations",
                () => new JValue(Physics2D.velocityIterations),
                v => Physics2D.velocityIterations = v.Value<int>());
            AddSettingPropertyHandler("positionIterations",
                () => new JValue(Physics2D.positionIterations),
                v => Physics2D.positionIterations = v.Value<int>());
            AddSettingPropertyHandler("velocityThreshold",
                () => new JValue(Physics2D.velocityThreshold),
                v => Physics2D.velocityThreshold = v.Value<float>());
            AddSettingPropertyHandler("maxLinearCorrection",
                () => new JValue(Physics2D.maxLinearCorrection),
                v => Physics2D.maxLinearCorrection = v.Value<float>());
            AddSettingPropertyHandler("maxAngularCorrection",
                () => new JValue(Physics2D.maxAngularCorrection),
                v => Physics2D.maxAngularCorrection = v.Value<float>());
            AddSettingPropertyHandler("maxTranslationSpeed",
                () => new JValue(Physics2D.maxTranslationSpeed),
                v => Physics2D.maxTranslationSpeed = v.Value<float>());
            AddSettingPropertyHandler("maxRotationSpeed",
                () => new JValue(Physics2D.maxRotationSpeed),
                v => Physics2D.maxRotationSpeed = v.Value<float>());
            AddSettingPropertyHandler("baumgarteScale",
                () => new JValue(Physics2D.baumgarteScale),
                v => Physics2D.baumgarteScale = v.Value<float>());
            AddSettingPropertyHandler("baumgarteTOIScale",
                () => new JValue(Physics2D.baumgarteTOIScale),
                v => Physics2D.baumgarteTOIScale = v.Value<float>());
            AddSettingPropertyHandler("timeToSleep",
                () => new JValue(Physics2D.timeToSleep),
                v => Physics2D.timeToSleep = v.Value<float>());
            AddSettingPropertyHandler("linearSleepTolerance",
                () => new JValue(Physics2D.linearSleepTolerance),
                v => Physics2D.linearSleepTolerance = v.Value<float>());
            AddSettingPropertyHandler("angularSleepTolerance",
                () => new JValue(Physics2D.angularSleepTolerance),
                v => Physics2D.angularSleepTolerance = v.Value<float>());
            AddSettingPropertyHandler("queriesHitTriggers",
                () => new JValue(Physics2D.queriesHitTriggers),
                v => Physics2D.queriesHitTriggers = v.Value<bool>());
            AddSettingPropertyHandler("queriesStartInColliders",
                () => new JValue(Physics2D.queriesStartInColliders),
                v => Physics2D.queriesStartInColliders = v.Value<bool>());
            AddSettingPropertyHandler("callbacksOnDisable",
                () => new JValue(Physics2D.callbacksOnDisable),
                v => Physics2D.callbacksOnDisable = v.Value<bool>());
            AddSettingPropertyHandler("reuseCollisionCallbacks",
                () => new JValue(Physics2D.reuseCollisionCallbacks),
                v => Physics2D.reuseCollisionCallbacks = v.Value<bool>());
            AddSettingPropertyHandler("autoSyncTransforms",
                () => new JValue(Physics2D.autoSyncTransforms),
                v => Physics2D.autoSyncTransforms = v.Value<bool>());
        }

        protected override void OnDumpRequested()
        {
            DumpProperty("gravity", Physics2D.gravity.SerializeToJObject());
            DumpProperty("defaultContactOffset", Physics2D.defaultContactOffset);
            DumpProperty("velocityIterations", Physics2D.velocityIterations);
            DumpProperty("positionIterations", Physics2D.positionIterations);
            DumpProperty("velocityThreshold", Physics2D.velocityThreshold);
            DumpProperty("maxLinearCorrection", Physics2D.maxLinearCorrection);
            DumpProperty("maxAngularCorrection", Physics2D.maxAngularCorrection);
            DumpProperty("maxTranslationSpeed", Physics2D.maxTranslationSpeed);
            DumpProperty("maxRotationSpeed", Physics2D.maxRotationSpeed);
            DumpProperty("baumgarteScale", Physics2D.baumgarteScale);
            DumpProperty("baumgarteTOIScale", Physics2D.baumgarteTOIScale);
            DumpProperty("timeToSleep", Physics2D.timeToSleep);
            DumpProperty("linearSleepTolerance", Physics2D.linearSleepTolerance);
            DumpProperty("angularSleepTolerance", Physics2D.angularSleepTolerance);
            DumpProperty("queriesHitTriggers", Physics2D.queriesHitTriggers);
            DumpProperty("queriesStartInColliders", Physics2D.queriesStartInColliders);
            DumpProperty("callbacksOnDisable", Physics2D.callbacksOnDisable);
            DumpProperty("reuseCollisionCallbacks", Physics2D.reuseCollisionCallbacks);
            DumpProperty("autoSyncTransforms", Physics2D.autoSyncTransforms);

            // Collision matrix (2D)
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
                    if (!Physics2D.GetIgnoreLayerCollision(i, j))
                        collidesWith.Add(nameB);
                }
                matrix[nameA] = collidesWith;
            }
            DumpProperty("collisionMatrix", matrix);
        }

        protected override void OnDefinitionRequested()
        {
            EmitClassDefinition("Physics2DSettings", new List<TsPropertyDef>
            {
                TsPropertyDef.Field("gravity", "Vector2"),
                TsPropertyDef.Field("defaultContactOffset", "number"),
                TsPropertyDef.Field("velocityIterations", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("positionIterations", "number")
                    .WithDecorator("type: Integer"),
                TsPropertyDef.Field("velocityThreshold", "number"),
                TsPropertyDef.Field("maxLinearCorrection", "number"),
                TsPropertyDef.Field("maxAngularCorrection", "number"),
                TsPropertyDef.Field("maxTranslationSpeed", "number"),
                TsPropertyDef.Field("maxRotationSpeed", "number"),
                TsPropertyDef.Field("baumgarteScale", "number"),
                TsPropertyDef.Field("baumgarteTOIScale", "number"),
                TsPropertyDef.Field("timeToSleep", "number"),
                TsPropertyDef.Field("linearSleepTolerance", "number"),
                TsPropertyDef.Field("angularSleepTolerance", "number"),
                TsPropertyDef.Field("queriesHitTriggers", "boolean"),
                TsPropertyDef.Field("queriesStartInColliders", "boolean"),
                TsPropertyDef.Field("callbacksOnDisable", "boolean"),
                TsPropertyDef.Field("reuseCollisionCallbacks", "boolean"),
                TsPropertyDef.Field("autoSyncTransforms", "boolean"),
                TsPropertyDef.Field("collisionMatrix", "Record<string, string[]>")
                    .WithComment("Per layer name → array of layer names it collides with (2D)"),
            });
        }
    }
}
