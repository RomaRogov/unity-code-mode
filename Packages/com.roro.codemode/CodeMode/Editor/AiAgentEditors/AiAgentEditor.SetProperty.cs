using System;
using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.AiAgentEditors
{
    public partial class AiAgentEditor
    {
        public void SetProperties(string[] propertyPaths, JToken[] values)
        {
            if (propertyPaths == null) throw new ArgumentNullException(nameof(propertyPaths));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (propertyPaths.Length != values.Length)
                throw new ArgumentException("Property paths and values arrays must have the same length.");

            serializedObject.Update();
            Undo.RecordObject(target, $"Set {propertyPaths.Length} properties");

            for (int i = 0; i < propertyPaths.Length; i++)
            {
                SetProperty(propertyPaths[i], values[i]);
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        public void SetProperty(string propertyPath, JToken value)
        {
            if (string.IsNullOrEmpty(propertyPath))
                throw new ArgumentNullException(nameof(propertyPath));

            var (handlerPath, remainingPath) = FindHandlerForPath(propertyPath);

            if (handlerPath != null)
            {
                var setHandler = _propertySetHandlers[handlerPath];

                if (_propertyGetHandlers != null &&
                    _propertyGetHandlers.TryGetValue(handlerPath, out var getHandler))
                {
                    var current = getHandler();
                    
                    // If we got reference object and have to go deeper - try to apply recursively with it's inspector
                    if (!string.IsNullOrEmpty(remainingPath) && current.Type == JTokenType.Object && current["id"] != null)
                    {
                        bool isValidReference = true;
                        JObject currentObj = (JObject)current;
                        foreach (var prop in currentObj.Properties())
                        {
                            if (prop.Name != "id" && prop.Name != "type")
                            {
                                isValidReference = false;
                                break;
                            }
                        }

                        if (isValidReference)
                        {
                            Object instance = GetInstanceFromJTokenReference(current);
                            var editor = CreateFor(instance);
                            editor.SetProperty(remainingPath, value);
                            return;
                        }
                    }
                    
                    var merged = ApplyPatch(current, remainingPath, value);
                    setHandler(merged);
                }
                else
                {
                    if (remainingPath != null)
                        throw new Exception(
                            $"Cannot set subpath '{remainingPath}' on '{handlerPath}' without a getHandler.");
                    setHandler(value);
                }
                
                return;
            }

            SetPropertyByPath(propertyPath, value);
        }

        #region Helpers

        private void SetPropertyByPath(string path, JToken jsonValue)
        {
            var data = SerializationUtils.ProcessSerializedObject(serializedObject);

            var segments = path.Split('.');
            var property = ResolvePath(data, segments, 0);

            if (property == null)
                throw new Exception($"Property '{path}' not found on {serializedObject.targetObject.GetType().Name}.");

            SetSerializedPropertyValue(property, jsonValue);
        }

        private SerializedProperty ResolvePath(
            SerializationUtils.SerializedObjectData data, string[] segments, int index)
        {
            if (index >= segments.Length) return null;

            var name = segments[index];
            bool isLast = index == segments.Length - 1;

            foreach (var p in data.properties)
            {
                if (p.name != name) continue;
                if (isLast) return p;
                var remaining = string.Join(".", segments, index + 1, segments.Length - index - 1);
                return p.serializedObject.FindProperty(p.propertyPath + "." + remaining);
            }

            foreach (var s in data.structs)
            {
                if (s.property.name != name) continue;
                if (isLast) return s.property;
                return ResolvePath(s.children, segments, index + 1);
            }

            foreach (var a in data.arrays)
            {
                if (a.property.name != name) continue;
                if (isLast) return a.property;

                if (index + 1 >= segments.Length || !int.TryParse(segments[index + 1], out int arrayIndex))
                    return null;
                if (arrayIndex < 0 || arrayIndex >= a.property.arraySize)
                    return null;

                if (index + 2 >= segments.Length)
                    return a.property.GetArrayElementAtIndex(arrayIndex);

                if (a.elements != null)
                    return ResolvePath(a.elements[arrayIndex], segments, index + 2);

                var elem = a.property.GetArrayElementAtIndex(arrayIndex);
                var rest = string.Join(".", segments, index + 2, segments.Length - index - 2);
                return elem.serializedObject.FindProperty(elem.propertyPath + "." + rest);
            }

            return null;
        }

        private void SetSerializedPropertyValue(SerializedProperty property, JToken jsonValue)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: property.intValue = jsonValue.Value<int>(); break;
                case SerializedPropertyType.Boolean: property.boolValue = jsonValue.Value<bool>(); break;
                case SerializedPropertyType.Float: property.floatValue = jsonValue.Value<float>(); break;
                case SerializedPropertyType.String: property.stringValue = jsonValue.Value<string>(); break;
                case SerializedPropertyType.Color: property.colorValue = jsonValue.DeserializeToColor(); break;
                case SerializedPropertyType.ObjectReference: SetObjectReferenceWithJTokenInstance(property, jsonValue); break;
                case SerializedPropertyType.LayerMask: property.intValue = jsonValue.Value<int>(); break;
                case SerializedPropertyType.Enum: SetEnumValue(property, jsonValue); break;
                case SerializedPropertyType.Vector2: property.vector2Value = jsonValue.DeserializeToVector2(); break;
                case SerializedPropertyType.Vector3: property.vector3Value = jsonValue.DeserializeToVector3(); break;
                case SerializedPropertyType.Vector4: property.vector4Value = jsonValue.DeserializeToVector4(); break;
                case SerializedPropertyType.Rect: property.rectValue = jsonValue.DeserializeToRect(); break;
                case SerializedPropertyType.ArraySize: property.intValue = jsonValue.Value<int>(); break;
                case SerializedPropertyType.Character: property.intValue = jsonValue.Value<int>(); break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = jsonValue.DeserializeToAnimationCurve(); break;
                case SerializedPropertyType.Bounds: property.boundsValue = jsonValue.DeserializeToBounds(); break;
                case SerializedPropertyType.Gradient: property.gradientValue = jsonValue.DeserializeToGradient(); break;
                case SerializedPropertyType.Quaternion: property.quaternionValue = jsonValue.DeserializeToQuaternion(); break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = GetInstanceFromJTokenReference(jsonValue); break;
                case SerializedPropertyType.FixedBufferSize: throw new Exception("FixedBufferSize is read-only.");
                case SerializedPropertyType.Vector2Int: property.vector2IntValue = jsonValue.DeserializeToVector2Int(); break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = jsonValue.DeserializeToVector3Int(); break;
                case SerializedPropertyType.RectInt: property.rectIntValue = jsonValue.DeserializeToRectInt(); break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = jsonValue.DeserializeToBoundsInt(); break;
                case SerializedPropertyType.ManagedReference:
                    SetManagedReference(property, jsonValue);
                    break;
                case SerializedPropertyType.Generic:
                    if (property.isArray)
                        SetArrayValue(property, jsonValue);
                    else
                        SetGenericPropertyValue(property, jsonValue);
                    break;

                default:
                    throw new Exception($"Unsupported property type: {property.propertyType}");
            }
        }

        private Object GetInstanceFromJTokenReference(JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            if (value["id"] == null)
            {
                throw new Exception("Reference object has invalid format. Expected an object with 'id' property.");
            }
            Object instance = EditorUtility.InstanceIDToObject(value["id"].Value<int>());
            if (instance == null)
            {
                throw new Exception("No Unity Object found with Instance ID " + value["id"].Value<int>());
            }
            
            return instance;
        }
        
        protected void SetObjectReferenceWithJTokenInstance(SerializedProperty property, JToken jsonValue)
        {
            Object instance = GetInstanceFromJTokenReference(jsonValue);
            if (instance == null)
            {
                property.objectReferenceValue = null;
                return;
            }
            string cleanType = property.type.Replace("PPtr<", "").Replace(">", "");
            Type expectedType = Type.GetType(cleanType);
            // Can't check for expected type -- just try to set property as fallback
            if (expectedType == null)
            {
                property.objectReferenceValue = instance;
                return;
            }
            if (!expectedType.IsAssignableFrom(instance.GetType()))
            {
                // Try to check if it's asset, so we can check subassets for the right type
                var path = AssetDatabase.GetAssetPath(instance);
                if (!string.IsNullOrEmpty(path))
                {
                    var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var subAsset in subAssets)
                    {
                        if (subAsset == instance || subAsset == null)
                        {
                            continue;
                        }
                        
                        if (expectedType.IsAssignableFrom(subAsset.GetType()))
                        {
                            property.objectReferenceValue = subAsset;
                            return;
                        }
                    }
                }
                
                // No assets found, type mismatch - throw an error
                throw new Exception($"Type mismatch: can't set '{instance.GetType().FullName}' to excepted type '{expectedType.FullName}'.");
            }
            
            property.objectReferenceValue = instance;
        }

        protected void SetEnumValue(SerializedProperty property, JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                property.enumValueIndex = 0;
                return;
            }

            if (value.Type == JTokenType.String)
            {
                string enumName = value.Value<string>();
                var index = Array.FindIndex(property.enumNames, 
                    en => en == enumName || SanitizeIdentifier(en) == enumName);
                if (index >= 0)
                {
                    property.enumValueIndex = index;
                    return;
                }

                if (int.TryParse(enumName, out int parsedIndex))
                {
                    if (parsedIndex >= 0 && parsedIndex < property.enumNames.Length)
                        property.enumValueIndex = parsedIndex;
                }
            }
            else if (value.Type == JTokenType.Integer)
            {
                var enumIndex = value.Value<int>();
                if (enumIndex >= 0 && enumIndex < property.enumNames.Length)
                    property.enumValueIndex = enumIndex;
            }
        }

        private void SetArrayValue(SerializedProperty arrayProperty, JToken value)
        {
            if (value is JArray jArray)
            {
                arrayProperty.arraySize = jArray.Count;

                for (int i = 0; i < jArray.Count; i++)
                {
                    var element = arrayProperty.GetArrayElementAtIndex(i);
                    SetSerializedPropertyValue(element, jArray[i]);
                }
            }
            else
            {
                throw new Exception($"Cannot set array property with value of type {value?.GetType().Name ?? "null"}");
            }
        }

        private void SetGenericPropertyValue(SerializedProperty property, JToken value)
        {
            if (value is not JObject jo)
                throw new Exception($"Cannot set generic property with value of type {value?.GetType().Name ?? "null"}");

            var data = SerializationUtils.ProcessSerializedProperty(property);

            foreach (var p in data.properties)
                if (jo.TryGetValue(p.name, out var v))
                    SetSerializedPropertyValue(p, v);

            foreach (var s in data.structs)
                if (jo.TryGetValue(s.property.name, out var v))
                    SetSerializedPropertyValue(s.property, v);

            foreach (var a in data.arrays)
                if (jo.TryGetValue(a.property.name, out var v))
                    SetSerializedPropertyValue(a.property, v);
        }

        private void SetManagedReference(SerializedProperty property, JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                property.managedReferenceValue = null;
                return;
            }

            if (property.managedReferenceValue != null)
            {
                SetGenericPropertyValue(property, value);
                return;
            }

            throw new Exception($"Cannot set managed reference at property '{property.propertyPath}'");
        }

        #endregion
    }
}
