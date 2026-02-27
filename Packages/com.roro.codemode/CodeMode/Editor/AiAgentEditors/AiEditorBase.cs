using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

namespace CodeMode.Editor.AiAgentEditors
{
    public abstract class AiEditorBase
    {
        protected virtual void OnEnable() { }

        protected string SanitizeIdentifier(string name)
        {
            var sanitized = Regex.Replace(name, "[^a-zA-Z0-9_]", "");
            if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;
            return sanitized;
        }

        #region Dump

        protected JObject dump;

        public JObject BuildDump()
        {
            dump = new JObject();
            OnDumpRequested();
            var result = dump;
            dump = null;
            return result;
        }

        protected virtual void OnDumpRequested() { }

        protected void DumpProperty(string name, JToken value)
        {
            dump[name] = value;
        }

        protected static JToken SerializeInstanceReferenceToJToken(Object obj)
        {
            if (obj == null)
                return JValue.CreateNull();

            return new JObject
            {
                ["id"] = obj.GetInstanceID().ToString(),
                ["type"] = obj.GetType().Name
            };
        }

        #endregion

        #region Definition

        protected List<string> _definitions;
        protected HashSet<string> _definedNames;

        public string BuildDefinition()
        {
            _definitions = new List<string>();
            _definedNames = new HashSet<string>();
            OnDefinitionRequested();
            var result = string.Join("\n", _definitions);
            _definitions = null;
            _definedNames = null;
            return result;
        }

        protected virtual void OnDefinitionRequested() { }

        protected void EmitClassDefinition(string className, List<TsPropertyDef> fields, string extends = null)
        {
            var lines = new List<string>();
            string extension = string.IsNullOrEmpty(extends) ? "" : $"extends {extends}";
            lines.Add($"export class {className} {extension} {{");
            foreach (var f in fields)
                f.Render(lines);
            lines.Add("}");
            _definitions.Add(string.Join("\n", lines));
        }

        protected void GenerateEnumDefinition(Type enumType)
        {
            var name = enumType.Name;
            if (_definedNames.Contains(name))
                return;
            _definedNames.Add(name);

            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType);
            var lines = new List<string>();
            lines.Add($"export enum {name} {{");
            for (int i = 0; i < names.Length; i++)
            {
                var cleanName = SanitizeIdentifier(names[i]);
                var val = Convert.ToInt32(values.GetValue(i));
                lines.Add($"\t{cleanName} = {val},");
            }
            lines.Add("}");

            _definitions.Insert(0, string.Join("\n", lines));
        }

        protected void EmitCustomEnumDefinition(string enumName, IEnumerable<KeyValuePair<string, string>> entries)
        {
            if (_definedNames.Contains(enumName))
                return;
            _definedNames.Add(enumName);

            var lines = new List<string>();
            lines.Add($"export enum {enumName} {{");
            foreach (var entry in entries)
            {
                string entryValue = string.IsNullOrEmpty(entry.Value) ? "" : $"= {entry.Value}";
                lines.Add($"\t{entry.Key} {entryValue},");
            }
                
            lines.Add("}");

            _definitions.Insert(0, string.Join("\n", lines));
        }

        #endregion

        #region Set Property

        protected Dictionary<string, Func<JToken>> _propertyGetHandlers;
        protected Dictionary<string, Action<JToken>> _propertySetHandlers;

        protected void AddSettingPropertyHandler(string path, Func<JToken> getHandler, Action<JToken> setHandler)
        {
            _propertyGetHandlers ??= new Dictionary<string, Func<JToken>>();
            _propertyGetHandlers[path] = getHandler;
            _propertySetHandlers ??= new Dictionary<string, Action<JToken>>();
            _propertySetHandlers[path] = setHandler;
        }

        protected (string handlerPath, string remainingPath) FindHandlerForPath(string path)
        {
            if (_propertySetHandlers == null)
                return (null, null);

            if (_propertySetHandlers.ContainsKey(path))
                return (path, null);

            foreach (var key in _propertySetHandlers.Keys)
            {
                if (path.StartsWith(key + "."))
                    return (key, path.Substring(key.Length + 1));
            }

            return (null, null);
        }

        protected static JToken ApplyPatch(JToken current, string subPath, JToken value)
        {
            if (subPath == null)
            {
                if (current is JObject currentObj && value is JObject valueObj)
                {
                    var merged = (JObject)currentObj.DeepClone();
                    merged.Merge(valueObj, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                    return merged;
                }

                return value;
            }

            var result = current.DeepClone();
            var segments = subPath.Split('.');
            var target = result;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                target = target[segments[i]];
                if (target == null)
                    throw new Exception($"Path segment '{segments[i]}' not found in property value");
            }

            target[segments[segments.Length - 1]] = value;
            return result;
        }

        #endregion
    }
}
