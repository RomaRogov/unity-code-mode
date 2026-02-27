using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using CodeMode.Editor.Server;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CodeMode.Editor.Tools.Registry
{
    /// <summary>
    /// Executes registered tools on the main thread.
    /// Fire-and-forget tools (void/UniTask) return immediately;
    /// value-returning tools block until the result is available.
    /// </summary>
    public class ToolExecution
    {
        private readonly ToolRegistry _registry;
        private readonly ConcurrentQueue<Action> _pendingActions = new();
        private bool _running;

        public ToolExecution(ToolRegistry registry)
        {
            _registry = registry;
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            EditorApplication.update += ProcessQueue;
        }

        public void Stop()
        {
            if (!_running) return;
            _running = false;
            EditorApplication.update -= ProcessQueue;
        }

        private void ProcessQueue()
        {
            while (_pendingActions.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CodeMode] Error processing queued tool action: {ex}");
                }
            }
        }

        public UniTask<RouteResult> ExecuteTool(string toolName, RequestContext context)
        {
            if (!_registry.Tools.TryGetValue(toolName, out var metadata))
                return UniTask.FromResult(RouteResult.NotFound($"Tool '{toolName}' not found"));

            if (metadata.IsFireAndForget)
                return ExecuteFireAndForget(metadata, toolName, context);

            return ExecuteWithResult(metadata, toolName, context);
        }

        private UniTask<RouteResult> ExecuteFireAndForget(ToolMetadata metadata, string toolName, RequestContext context)
        {
            _pendingActions.Enqueue(() =>
            {
                try
                {
                    var args = BuildArgs(metadata, context);
                    var result = metadata.Method.Invoke(null, args);

                    // If it returns a UniTask (non-generic), we still need to observe it for exceptions
                    if (result is UniTask unitask)
                    {
                        ObserveUniTask(unitask, toolName).Forget();
                    }
                }
                catch (TargetInvocationException ex)
                {
                    Debug.LogError($"[CodeMode] Fire-and-forget tool '{toolName}' error: {ex.InnerException?.Message ?? ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CodeMode] Fire-and-forget tool '{toolName}' error: {ex.Message}");
                }
            });

            return UniTask.FromResult(RouteResult.Ok(new { success = true }));
        }

        private UniTask<RouteResult> ExecuteWithResult(ToolMetadata metadata, string toolName, RequestContext context)
        {
            var tcs = new UniTaskCompletionSource<RouteResult>();

            _pendingActions.Enqueue(() =>
            {
                try
                {
                    var args = BuildArgs(metadata, context);
                    var result = metadata.Method.Invoke(null, args);

                    if (metadata.UniTaskAwaiter != null && result != null)
                    {
                        // Async tool — use pre-built awaiter delegate (no per-call reflection)
                        CompleteFromUniTask(metadata.UniTaskAwaiter(result), toolName, tcs).Forget();
                    }
                    else
                    {
                        tcs.TrySetResult(RouteResult.Ok(result));
                    }
                }
                catch (TargetInvocationException ex)
                {
                    tcs.TrySetResult(RouteResult.InternalError(
                        $"Unity tool {toolName} error: {ex.InnerException?.Message ?? ex.Message}"));
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(RouteResult.InternalError($"Unity tool {toolName} error: {ex.Message}"));
                }
            });

            return tcs.Task;
        }

        private static async UniTaskVoid CompleteFromUniTask(
            UniTask<object> task, string toolName, UniTaskCompletionSource<RouteResult> tcs)
        {
            try
            {
                var result = await task;
                tcs.TrySetResult(RouteResult.Ok(result));
            }
            catch (Exception ex)
            {
                tcs.TrySetResult(RouteResult.InternalError($"Unity tool {toolName} error: {ex.Message}"));
            }
        }

        private static async UniTaskVoid ObserveUniTask(UniTask task, string toolName)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CodeMode] Fire-and-forget tool '{toolName}' async error: {ex.Message}");
            }
        }

        #region Argument Building

        private object[] BuildArgs(ToolMetadata metadata, RequestContext context)
        {
            if (metadata.UsesInputClass)
            {
                var input = BuildClassInput(metadata, context);
                return input != null ? new[] { input } : Array.Empty<object>();
            }

            return BuildParameterArgs(metadata, context);
        }

        private object BuildClassInput(ToolMetadata metadata, RequestContext context)
        {
            var input = Activator.CreateInstance(metadata.InputType);

            foreach (var field in metadata.InputType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.Name == metadata.BodyFieldName)
                {
                    object bodyValue = null;
                    if (!string.IsNullOrEmpty(context.Body))
                        bodyValue = ParseBodyField(context.Body, field.FieldType);

                    if (bodyValue == null)
                    {
                        var fallback = context.GetParam(field.Name);
                        if (fallback != null)
                            bodyValue = ConvertValue(fallback, field.FieldType);
                    }

                    if (bodyValue != null)
                        field.SetValue(input, bodyValue);
                    continue;
                }

                var paramValue = context.GetParam(field.Name);
                if (paramValue == null) continue;

                var converted = ConvertValue(paramValue, field.FieldType);
                if (converted != null)
                {
                    field.SetValue(input, converted);
                }
                else if (field.FieldType.IsEnum)
                {
                    var validValues = string.Join(", ", Enum.GetNames(field.FieldType));
                    throw new Exception(
                        $"Invalid value '{paramValue}' for enum field '{field.Name}'. Valid values: {validValues}");
                }
            }

            return input;
        }

        private object[] BuildParameterArgs(ToolMetadata metadata, RequestContext context)
        {
            if (metadata.Parameters == null || metadata.Parameters.Length == 0)
                return Array.Empty<object>();

            var args = new object[metadata.Parameters.Length];

            for (int i = 0; i < metadata.Parameters.Length; i++)
            {
                var param = metadata.Parameters[i];
                object value = null;

                if (param.Name == metadata.BodyFieldName)
                {
                    if (!string.IsNullOrEmpty(context.Body))
                        value = ParseBodyField(context.Body, param.ParameterType);

                    if (value == null)
                    {
                        var fallback = context.GetParam(param.Name);
                        if (fallback != null)
                            value = ConvertValue(fallback, param.ParameterType);
                    }
                }
                else
                {
                    var paramValue = context.GetParam(param.Name);
                    if (paramValue != null)
                    {
                        value = ConvertValue(paramValue, param.ParameterType);

                        if (value == null && param.ParameterType.IsEnum)
                        {
                            var validValues = string.Join(", ", Enum.GetNames(param.ParameterType));
                            throw new Exception(
                                $"Invalid value '{paramValue}' for enum parameter '{param.Name}'. Valid values: {validValues}");
                        }
                    }
                }

                if (value == null && param.HasDefaultValue)
                    value = param.DefaultValue;

                if (value == null && param.ParameterType.IsValueType)
                    value = Activator.CreateInstance(param.ParameterType);

                args[i] = value;
            }

            return args;
        }

        private object ParseBodyField(string body, Type fieldType)
        {
            if (string.IsNullOrEmpty(body)) return null;

            if (fieldType.IsValueType || Nullable.GetUnderlyingType(fieldType) != null)
                return ConvertQueryParam(body.Trim(), fieldType);

            try
            {
                return JsonConvert.DeserializeObject(body, fieldType);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CodeMode] Failed to parse JSON body: {ex.Message}");
                return null;
            }
        }

        private object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;

            if (targetType.IsInstanceOfType(value))
                return value;

            if (value is string strValue)
                return ConvertQueryParam(strValue, targetType);

            if (value is Dictionary<string, object> || value is List<object>)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(value);
                    return JsonConvert.DeserializeObject(json, targetType);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[CodeMode] Failed to convert nested value to {targetType.Name}: {ex.Message}");
                    return null;
                }
            }

            try
            {
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
            }
            catch
            {
                return null;
            }
        }

        private object ConvertQueryParam(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var underlying = Nullable.GetUnderlyingType(targetType);
            if (underlying != null)
                return ConvertQueryParam(value, underlying);

            if (targetType == typeof(string)) return value;

            if (targetType == typeof(bool))
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1";

            if (targetType == typeof(int) && int.TryParse(value, out var i)) return i;
            if (targetType == typeof(long) && long.TryParse(value, out var l)) return l;
            if (targetType == typeof(float) && float.TryParse(value, out var f)) return f;
            if (targetType == typeof(double) && double.TryParse(value, out var d)) return d;

            if (targetType.IsEnum && Enum.TryParse(targetType, value, true, out var e)) return e;

            return null;
        }

        #endregion
    }
}
