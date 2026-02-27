using System;
using System.Collections.Generic;

namespace CodeMode.Editor.Protocol
{
    /// <summary>
    /// JSON Schema definition (based on draft-07).
    /// This defines the structure for tool inputs and outputs.
    /// </summary>
    [Serializable]
    public class JsonSchema
    {
        public string type;
        public string description;
        public Dictionary<string, JsonSchema> properties;
        public List<string> required;
        public JsonSchema items;
        public List<string> @enum;
        public object @default;
        public string @const;
        public bool? nullable;
        public double? minimum;
        public double? maximum;

        #region Factory Methods

        public static JsonSchema String(string description = null)
        {
            return new JsonSchema { type = "string", description = description };
        }

        public static JsonSchema Integer(string description = null)
        {
            return new JsonSchema { type = "integer", description = description };
        }

        public static JsonSchema Number(string description = null)
        {
            return new JsonSchema { type = "number", description = description };
        }

        public static JsonSchema Boolean(string description = null)
        {
            return new JsonSchema { type = "boolean", description = description };
        }

        public static JsonSchema Object(string description = null)
        {
            return new JsonSchema
            {
                type = "object",
                description = description,
                properties = new Dictionary<string, JsonSchema>(),
                required = new List<string>()
            };
        }

        public static JsonSchema Array(JsonSchema items, string description = null)
        {
            return new JsonSchema
            {
                type = "array",
                description = description,
                items = items
            };
        }

        public static JsonSchema Enum(string description, params string[] values)
        {
            return new JsonSchema
            {
                type = "string",
                description = description,
                @enum = new List<string>(values)
            };
        }

        #endregion

        #region Fluent Builder Methods

        public JsonSchema WithDescription(string desc)
        {
            description = desc;
            return this;
        }

        public JsonSchema WithDefault(object value)
        {
            @default = value;
            return this;
        }

        public JsonSchema WithNullable(bool value = true)
        {
            nullable = value;
            return this;
        }

        public JsonSchema WithRange(double? min = null, double? max = null)
        {
            minimum = min;
            maximum = max;
            return this;
        }

        public JsonSchema Prop(string name, JsonSchema schema, bool isRequired = false)
        {
            if (properties == null) properties = new Dictionary<string, JsonSchema>();
            if (required == null) required = new List<string>();

            properties[name] = schema;
            if (isRequired && !required.Contains(name))
            {
                required.Add(name);
            }
            return this;
        }

        #endregion

        #region Serialization

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(type)) dict["type"] = type;
            if (!string.IsNullOrEmpty(description)) dict["description"] = description;
            if (!string.IsNullOrEmpty(@const)) dict["const"] = @const;
            if (@default != null) dict["default"] = @default;
            if (nullable.HasValue) dict["nullable"] = nullable.Value;
            if (minimum.HasValue) dict["minimum"] = minimum.Value;
            if (maximum.HasValue) dict["maximum"] = maximum.Value;

            if (@enum != null && @enum.Count > 0)
            {
                dict["enum"] = @enum;
            }

            if (properties != null && properties.Count > 0)
            {
                var propsDict = new Dictionary<string, object>();
                foreach (var kvp in properties)
                {
                    propsDict[kvp.Key] = kvp.Value.ToDictionary();
                }
                dict["properties"] = propsDict;
            }

            if (required != null && required.Count > 0)
            {
                dict["required"] = required;
            }

            if (items != null)
            {
                dict["items"] = items.ToDictionary();
            }

            return dict;
        }

        #endregion
    }
}