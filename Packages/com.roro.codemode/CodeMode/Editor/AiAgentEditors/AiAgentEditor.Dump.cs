using CodeMode.Editor.Utilities;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace CodeMode.Editor.AiAgentEditors
{
    public partial class AiAgentEditor
    {
        protected override void OnDumpRequested()
        {
            serializedObject.Update();
            DumpSerializedProperties(serializedObject);
        }

        #region Dump Helpers

        protected void DumpSerializedProperties(SerializedObject so)
        {
            var data = SerializationUtils.ProcessSerializedObject(so);
            DumpSerializedData(data, dump);
        }

        protected static void DumpSerializedData(SerializationUtils.SerializedObjectData data, JObject target)
        {
            foreach (var p in data.properties)
                target[p.name] = SerializePropertyValue(p);

            foreach (var s in data.structs)
                target[s.property.name] = DataToJson(s.children);

            foreach (var a in data.arrays)
            {
                var jArray = new JArray();
                if (a.elements != null)
                {
                    foreach (var element in a.elements)
                        jArray.Add(DataToJson(element));
                }
                else
                {
                    for (int i = 0; i < a.property.arraySize; i++)
                        jArray.Add(SerializePropertyValue(a.property.GetArrayElementAtIndex(i)));
                }
                target[a.property.name] = jArray;
            }
        }

        protected static JObject DataToJson(SerializationUtils.SerializedObjectData data)
        {
            var result = new JObject();
            DumpSerializedData(data, result);
            return result;
        }
        
        protected static JValue SerializeEnumToJValue(SerializedProperty property)
        {
            if (property.enumNames.Length > 0 &&
                property.enumValueIndex >= 0 &&
                property.enumValueIndex < property.enumNames.Length)
            {
                return new JValue(property.enumNames[property.enumValueIndex]);
            }
            return new JValue(property.enumValueIndex);
        }

        protected static JToken SerializePropertyValue(SerializedProperty property)
        {
            if (property == null)
                return JValue.CreateNull();

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return new JValue(property.intValue);
                case SerializedPropertyType.Boolean: return new JValue(property.boolValue);
                case SerializedPropertyType.Float: return new JValue(property.floatValue);
                case SerializedPropertyType.String: return new JValue(property.stringValue);
                case SerializedPropertyType.LayerMask: return new JValue(property.intValue);
                case SerializedPropertyType.Character: return new JValue((char)property.intValue);
                case SerializedPropertyType.FixedBufferSize: return new JValue(property.fixedBufferSize);
                case SerializedPropertyType.Color: return property.colorValue.SerializeToJObject();
                case SerializedPropertyType.Vector2: return property.vector2Value.SerializeToJObject();
                case SerializedPropertyType.Vector3: return property.vector3Value.SerializeToJObject();
                case SerializedPropertyType.Vector4: return property.vector4Value.SerializeToJObject();
                case SerializedPropertyType.Quaternion: return property.quaternionValue.SerializeToJObject();
                case SerializedPropertyType.Rect: return property.rectValue.SerializeToJObject();
                case SerializedPropertyType.AnimationCurve: return property.animationCurveValue.SerializeToJObject();
                case SerializedPropertyType.Bounds: return property.boundsValue.SerializeToJObject();
                case SerializedPropertyType.Gradient: return property.gradientValue.SerializeToJObject();
                case SerializedPropertyType.Vector2Int: return property.vector2IntValue.SerializeToJObject();
                case SerializedPropertyType.Vector3Int: return property.vector3IntValue.SerializeToJObject();
                case SerializedPropertyType.RectInt: return property.rectIntValue.SerializeToJObject();
                case SerializedPropertyType.BoundsInt: return property.boundsIntValue.SerializeToJObject();
                case SerializedPropertyType.ExposedReference: return SerializeInstanceReferenceToJToken(property.exposedReferenceValue);
                case SerializedPropertyType.ObjectReference: return SerializeInstanceReferenceToJToken(property.objectReferenceValue);
                case SerializedPropertyType.Enum: return SerializeEnumToJValue(property);
                default:
                    return JValue.CreateNull();
            }
        }

        #endregion
    }
}
