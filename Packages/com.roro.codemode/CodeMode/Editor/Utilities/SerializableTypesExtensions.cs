using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CodeMode.Editor.Utilities
{
    /// <summary>
    /// Helper for safe serialization/deserialization of types that SerializedProperty provides
    /// </summary>
    public static class SerializableTypesExtensions
    {
        #region Vector2 / Vector2Int

        public static JObject SerializeToJObject(this Vector2 v)
        {
            return new JObject { ["x"] = v.x, ["y"] = v.y };
        }
        
        public static Vector2 DeserializeToVector2(this JToken j)
        {
            return new Vector2((float)j["x"], (float)j["y"]);
        }
        
        public static JObject SerializeToJObject(this Vector2Int v)
        {
            return new JObject { ["x"] = v.x, ["y"] = v.y };
        }
        
        public static Vector2Int DeserializeToVector2Int(this JToken j)
        {
            return new Vector2Int((int)j["x"], (int)j["y"]);
        }

        #endregion

        #region Vector3 / Vector3Int

        public static JObject SerializeToJObject(this Vector3 v)
        {
            return new JObject { ["x"] = v.x, ["y"] = v.y, ["z"] = v.z };
        }

        public static Vector3 DeserializeToVector3(this JToken j)
        {
            return new Vector3((float)j["x"], (float)j["y"], (float)j["z"]);
        }
        
        public static JObject SerializeToJObject(this Vector3Int v)
        {
            return new JObject { ["x"] = v.x, ["y"] = v.y, ["z"] = v.z };
        }
        
        public static Vector3Int DeserializeToVector3Int(this JToken j)
        {
            return new Vector3Int((int)j["x"], (int)j["y"], (int)j["z"]);
        }

        #endregion

        #region Vector4 / Quaternion

        public static JObject SerializeToJObject(this Vector4 v)
        {
            return new JObject { ["x"] = v.x, ["y"] = v.y, ["z"] = v.z, ["w"] = v.w };
        }

        public static Vector4 DeserializeToVector4(this JToken j)
        {
            return new Vector4((float)j["x"], (float)j["y"], (float)j["z"], (float)j["w"]);
        }

        public static JObject SerializeToJObject(this Quaternion q)
        {
            return new JObject { ["x"] = q.x, ["y"] = q.y, ["z"] = q.z, ["w"] = q.w };
        }
        
        public static Quaternion DeserializeToQuaternion(this JToken j)
        {
            return new Quaternion((float)j["x"], (float)j["y"], (float)j["z"], (float)j["w"]);
        }

        #endregion
        
        #region Color
        
        public static JObject SerializeToJObject(this Color color)
        {
            return new JObject { ["r"] = color.r, ["g"] = color.g, ["b"] = color.b, ["a"] = color.a };
        }
        
        public static Color DeserializeToColor(this JToken j)
        {
            return new Color((float)j["r"], (float)j["g"], (float)j["b"], (float)j["a"]);
        }
        
        #endregion

        #region Rect / RectInt

        public static JObject SerializeToJObject(this Rect rect)
        {
            return new JObject { ["x"] = rect.x, ["y"] = rect.y, ["width"] = rect.width, ["height"] = rect.height };
        }

        public static Rect DeserializeToRect(this JToken j)
        {
            return new Rect((float)j["x"], (float)j["y"], (float)j["width"], (float)j["height"]);
        }

        public static JObject SerializeToJObject(this RectInt rect)
        {
            return new JObject { ["y"] = rect.y, ["width"] = rect.width, ["height"] = rect.height };
        }

        public static RectInt DeserializeToRectInt(this JToken j)
        {
            return new RectInt((int)j["x"], (int)j["y"], (int)j["width"], (int)j["height"]);
        }
        
        #endregion

        #region Bounds / BoundsInt

        public static JObject SerializeToJObject(this Bounds bounds)
        {
            return new JObject { ["center"] = bounds.center.SerializeToJObject(), ["size"] = bounds.size.SerializeToJObject() };
        }

        public static Bounds DeserializeToBounds(this JToken j)
        {
            return new Bounds(j["center"].DeserializeToVector3(), j["size"].DeserializeToVector3());
        }

        public static JObject SerializeToJObject(this BoundsInt bounds)
        {
            return new JObject { ["position"] = bounds.position.SerializeToJObject() ["size"] = bounds.size.SerializeToJObject() };
        }
        
        public static BoundsInt DeserializeToBoundsInt(this JToken j)
        {
            return new BoundsInt(j["position"].DeserializeToVector3Int(), j["size"].DeserializeToVector3Int());
        }
        
        #endregion

        #region AnimationCurve

        public static JObject SerializeToJObject(this AnimationCurve curve)
        {
            var keys = new JArray();
            foreach (var keyframe in curve.keys)
            {
                keys.Add(new JObject
                {
                    ["time"] = keyframe.time,
                    ["value"] = keyframe.value,
                    ["inTangent"] = keyframe.inTangent,
                    ["outTangent"] = keyframe.outTangent,
                    ["inWeight"] = keyframe.inWeight,
                    ["outWeight"] = keyframe.outWeight,
                    ["weightedMode"] = keyframe.weightedMode.ToString()
                });
            }

            return new JObject
            {
                ["keys"] = keys,
                ["preWrapMode"] = curve.preWrapMode.ToString(),
                ["postWrapMode"] = curve.postWrapMode.ToString()
            };
        }

        public static AnimationCurve DeserializeToAnimationCurve(this JToken j)
        {
            var curve = new AnimationCurve();

            if (j["keys"] is JArray keysArray)
            {
                foreach (var keyToken in keysArray)
                {
                    if (keyToken is JObject keyObj)
                    {
                        var time = keyObj["time"]?.Value<float>() ?? 0f;
                        var val = keyObj["value"]?.Value<float>() ?? 0f;
                        var inTangent = keyObj["inTangent"]?.Value<float>() ?? 0f;
                        var outTangent = keyObj["outTangent"]?.Value<float>() ?? 0f;
                        
                        var keyframe = new Keyframe(time, val, inTangent, outTangent);
                        
                        // Set weights if available
                        if (keyObj["inWeight"] != null)
                            keyframe.inWeight = keyObj["inWeight"].Value<float>();
                        if (keyObj["outWeight"] != null)
                            keyframe.outWeight = keyObj["outWeight"].Value<float>();
                        
                        curve.AddKey(keyframe);
                    }
                }
            }
            
            // Set wrap modes if available
            if (j["preWrapMode"] != null && Enum.TryParse<WrapMode>(j["preWrapMode"].Value<string>(), out var preWrap))
                curve.preWrapMode = preWrap;
            if (j["postWrapMode"] != null && Enum.TryParse<WrapMode>(j["postWrapMode"].Value<string>(), out var postWrap))
                curve.postWrapMode = postWrap;
            
            return curve;
        }
        
        #endregion

        #region Gradient
        
        public static JObject SerializeToJObject(this Gradient gradient)
        {
            var alphaKeys = new JArray();
            foreach (var key in gradient.alphaKeys)
            {
                alphaKeys.Add(new JObject
                {
                    ["time"] = key.time,
                    ["alpha"] = key.alpha
                });
            }

            var colorKeys = new JArray();
            foreach (var key in gradient.colorKeys)
            {
                colorKeys.Add(new JObject
                {
                    ["time"] = key.time,
                    ["color"] = key.color.SerializeToJObject()
                });
            }

            return new JObject
            {
                ["alphaKeys"] = alphaKeys,
                ["colorKeys"] = colorKeys,
                ["colorSpace"] = gradient.colorSpace.ToString(),
                ["mode"] = gradient.mode.ToString()
            };
        }

        public static Gradient DeserializeToGradient(this JToken j)
        {
            var gradient = new Gradient();
            if (j["alphaKeys"] is JArray alphaKeysArray)
            {
                var alphaKeys = new GradientAlphaKey[alphaKeysArray.Count];
                for (int i = 0; i < alphaKeysArray.Count; i++)
                {
                    var keyObj = (JObject)alphaKeysArray[i];
                    alphaKeys[i] = new GradientAlphaKey
                    {
                        time = keyObj["time"]?.Value<float>() ?? 0f,
                        alpha = keyObj["alpha"]?.Value<float>() ?? 0f
                    };
                }
                gradient.alphaKeys = alphaKeys;
            }

            if (j["colorKeys"] is JArray colorKeysArray)
            {
                var colorKeys = new GradientColorKey[colorKeysArray.Count];
                for (int i = 0; i < colorKeysArray.Count; i++)
                {
                    var keyObj = (JObject)colorKeysArray[i];
                    colorKeys[i] = new GradientColorKey
                    {
                        time = keyObj["time"]?.Value<float>() ?? 0f,
                        color = keyObj["color"]?.DeserializeToColor() ?? Color.white
                    };
                }
                gradient.colorKeys = colorKeys;
            }

            j["colorSpace"].Value<Vector2>();
            
            if (j["colorSpace"] != null && Enum.TryParse<ColorSpace>(j["colorSpace"].Value<string>(), out var colorSpace))
                gradient.colorSpace = colorSpace;
            if (j["mode"] != null && Enum.TryParse<GradientMode>(j["mode"].Value<string>(), out var mode))
                gradient.mode = mode;
            
            return gradient;
        }
        
        #endregion
    }
}