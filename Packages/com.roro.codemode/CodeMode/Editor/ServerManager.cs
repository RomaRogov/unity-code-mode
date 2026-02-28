using System;
using System.Threading.Tasks;
using CodeMode.Editor.Config;
using CodeMode.Editor.Server;
using CodeMode.Editor.Tools.Registry;
using UnityEditor;
using UnityEngine;
using Tool = CodeMode.Editor.Protocol.Tool;

namespace CodeMode.Editor
{
    [InitializeOnLoad]
    public class ServerManager
    {
        private static ServerManager _instance;
        public static ServerManager Instance => _instance;

        private UtcpHttpServer _server;
        private ToolRegistry _toolRegistry;
        private ToolExecution _toolExecution;
        private UtcpConfig _config;
        private string _baseUrl;
        private int _actualPort;

        public bool IsRunning => _server?.IsRunning ?? false;
        public int ActualPort => _actualPort;
        public ToolRegistry ToolRegistry => _toolRegistry;

        public event Action<string> OnLog;

        static ServerManager()
        {
            _instance = new ServerManager();
            _instance.Initialize();
        }

        private void Initialize()
        {
            _config = UtcpConfigManager.LoadConfig();

            _toolRegistry = new ToolRegistry();
            _toolRegistry.BuildRegistry();
            _toolExecution = new ToolExecution(_toolRegistry);

            EditorApplication.quitting += OnEditorQuitting;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            if (_config.autoStart)
            {
                EditorApplication.delayCall += () =>
                {
                    if (!IsRunning)
                        StartServer();
                };
            }
        }

        public void StartServer()
        {
            if (IsRunning)
            {
                Log("Server is already running");
                return;
            }

            _config = UtcpConfigManager.LoadConfig();

            try
            {
                _server = new UtcpHttpServer();
                _server.OnLog += Log;
                _server.OnError += LogError;

                _actualPort = _server.Start(_config.address, _config.port);
                if (_config.port != _actualPort)
                {
                    _config.port = _actualPort;
                    UtcpConfigManager.SaveConfig(_config);
                }
                _baseUrl = $"http://{_config.address}:{_actualPort}";

                _toolExecution.Start();
                SetupRoutes();
                UtcpConfigManager.EnsureUnityTemplate(_actualPort);
            }
            catch (Exception ex)
            {
                LogError($"Failed to start server: {ex.Message}");
                _server?.Dispose();
                _server = null;
                _actualPort = 0;
            }
        }

        public void StopServer()
        {
            _toolExecution?.Stop();

            if (_server != null)
            {
                _server.OnLog -= Log;
                _server.OnError -= LogError;
                _server.Stop();
                _server.Dispose();
                _server = null;
            }
        }

        public void RestartServer()
        {
            StopServer();
            _toolRegistry.BuildRegistry();
            _toolExecution = new ToolExecution(_toolRegistry);
            StartServer();
        }

        private void SetupRoutes()
        {
            var router = _server.Router;

            // UTCP Manual endpoint — returns full tool manifest (dispatched to main thread)
            router.Get("/utcp", _ =>
                _toolExecution.RunOnMainThread(() =>
                    RouteResult.Ok(_toolRegistry.GetUtcpManual(_baseUrl, "1.0.0"))));

            // Health check
            router.Get("/health", _ =>
                Task.FromResult(RouteResult.Ok(new HealthResponse
                {
                    status = "ok",
                    timestamp = DateTime.UtcNow.ToString("o")
                })));

            // Tool execution — GET
            router.Get("/tools/{toolName}", ctx =>
            {
                var toolName = ctx.GetParam("toolName")?.ToString();
                return _toolExecution.ExecuteTool(toolName, ctx);
            });

            // Tool execution — POST
            router.Post("/tools/{toolName}", ctx =>
            {
                var toolName = ctx.GetParam("toolName")?.ToString();
                return _toolExecution.ExecuteTool(toolName, ctx);
            });

            // List all tools
            router.Get("/tools", _ =>
                _toolExecution.RunOnMainThread(() =>
                {
                    var manual = _toolRegistry.GetUtcpManual(_baseUrl, "1.0.0");
                    return RouteResult.Ok(new ToolListResponse { tools = manual.tools });
                }));
        }

        private void OnEditorQuitting() => StopServer();
        private void OnBeforeAssemblyReload() => StopServer();

        private void Log(string message)
        {
            if (_config?.logRequests == true)
                Debug.Log($"[UTCP] {message}");
            OnLog?.Invoke(message);
        }

        private void LogError(string message)
        {
            Debug.LogError($"[UTCP] {message}");
            OnLog?.Invoke($"ERROR: {message}");
        }

        [Serializable]
        private class HealthResponse
        {
            public string status;
            public string timestamp;
        }

        [Serializable]
        private class ToolListResponse
        {
            public System.Collections.Generic.List<Tool> tools;
        }
    }
}
