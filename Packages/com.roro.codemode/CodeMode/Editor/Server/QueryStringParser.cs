using System;
using System.Collections.Generic;

namespace CodeMode.Editor.Server
{
    /// <summary>
    /// Parses query strings with qs.parse-style bracket notation into nested dictionaries/lists.
    /// e.g. "reference[id]=123&reference[type]=Component"
    /// → { "reference": { "id": "123", "type": "Component" } }
    /// e.g. "items[]=a&items[]=b" → { "items": ["a", "b"] }
    /// e.g. "items[0][x]=1&items[1][x]=2" → { "items": [{ "x": "1" }, { "x": "2" }] }
    /// </summary>
    public static class QueryStringParser
    {
        public static Dictionary<string, object> Parse(string queryString)
        {
            var result = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(queryString))
                return result;

            queryString = queryString.TrimStart('?');

            foreach (var param in queryString.Split('&'))
            {
                if (string.IsNullOrEmpty(param))
                    continue;

                var parts = param.Split(new[] { '=' }, 2);
                var rawKey = Uri.UnescapeDataString(parts[0].Replace('+', ' '));
                var rawValue = parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace('+', ' ')) : "";

                var segments = ParseKey(rawKey);
                var decoded = DecodeValue(rawValue);

                SetNested(result, segments, 0, decoded);
            }

            return result;
        }

        /// <summary>
        /// Parse bracket notation key into path segments.
        /// "field[a][b]" → ["field", "a", "b"]
        /// "simple" → ["simple"]
        /// "items[]" → ["items", ""]
        /// </summary>
        private static List<string> ParseKey(string key)
        {
            var segments = new List<string>();

            var bracketStart = key.IndexOf('[');
            if (bracketStart < 0)
            {
                segments.Add(key);
                return segments;
            }

            // Root segment before first bracket
            segments.Add(key.Substring(0, bracketStart));

            // Parse bracket segments
            var i = bracketStart;
            while (i < key.Length)
            {
                if (key[i] != '[')
                    break;

                var close = key.IndexOf(']', i + 1);
                if (close < 0)
                    break;

                segments.Add(key.Substring(i + 1, close - i - 1));
                i = close + 1;
            }

            return segments;
        }

        /// <summary>
        /// Decode value: keeps values as strings so that type coercion can happen later
        /// with knowledge of the target type. Only handles the __null__ sentinel here.
        /// </summary>
        private static object DecodeValue(string value)
        {
            if (value == "__null__") return null;
            return value;
        }

        /// <summary>
        /// Check if a segment represents an array index (empty string or numeric).
        /// </summary>
        private static bool IsArraySegment(string segment)
        {
            return segment.Length == 0 || IsNumeric(segment);
        }

        private static bool IsNumeric(string s)
        {
            foreach (var c in s)
                if (c < '0' || c > '9') return false;
            return s.Length > 0;
        }

        /// <summary>
        /// Recursively set a value at a nested path, auto-creating Lists for array
        /// segments (empty or numeric) and Dictionaries for named segments.
        /// </summary>
        private static void SetNested(object container, List<string> segments, int index, object value)
        {
            if (index >= segments.Count) return;

            var seg = segments[index];
            var isLast = index == segments.Count - 1;

            if (container is Dictionary<string, object> dict)
            {
                if (isLast)
                {
                    dict[seg] = value;
                    return;
                }

                // Peek at next segment to decide container type
                var nextSeg = segments[index + 1];
                var nextIsArray = IsArraySegment(nextSeg);

                if (dict.TryGetValue(seg, out var existing))
                {
                    if (nextIsArray && existing is List<object> existingList)
                    {
                        SetNested(existingList, segments, index + 1, value);
                    }
                    else if (!nextIsArray && existing is Dictionary<string, object> existingDict)
                    {
                        SetNested(existingDict, segments, index + 1, value);
                    }
                    else
                    {
                        // Type mismatch — replace with correct container
                        if (nextIsArray)
                        {
                            var list = new List<object>();
                            dict[seg] = list;
                            SetNested(list, segments, index + 1, value);
                        }
                        else
                        {
                            var newDict = new Dictionary<string, object>();
                            dict[seg] = newDict;
                            SetNested(newDict, segments, index + 1, value);
                        }
                    }
                }
                else
                {
                    if (nextIsArray)
                    {
                        var list = new List<object>();
                        dict[seg] = list;
                        SetNested(list, segments, index + 1, value);
                    }
                    else
                    {
                        var newDict = new Dictionary<string, object>();
                        dict[seg] = newDict;
                        SetNested(newDict, segments, index + 1, value);
                    }
                }
            }
            else if (container is List<object> list)
            {
                if (seg.Length == 0)
                {
                    // Empty bracket [] — append
                    if (isLast)
                    {
                        list.Add(value);
                    }
                    else
                    {
                        var nextSeg = segments[index + 1];
                        var nextIsArray = IsArraySegment(nextSeg);
                        object child = nextIsArray ? (object)new List<object>() : new Dictionary<string, object>();
                        list.Add(child);
                        SetNested(child, segments, index + 1, value);
                    }
                }
                else if (IsNumeric(seg))
                {
                    var idx = int.Parse(seg);

                    // Extend list if needed
                    while (list.Count <= idx)
                        list.Add(null);

                    if (isLast)
                    {
                        list[idx] = value;
                    }
                    else
                    {
                        var nextSeg = segments[index + 1];
                        var nextIsArray = IsArraySegment(nextSeg);

                        if (list[idx] == null)
                        {
                            object child = nextIsArray ? (object)new List<object>() : new Dictionary<string, object>();
                            list[idx] = child;
                        }

                        SetNested(list[idx], segments, index + 1, value);
                    }
                }
            }
        }
    }
}
