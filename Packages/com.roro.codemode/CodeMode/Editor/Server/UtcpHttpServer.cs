using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace CodeMode.Editor.Server
{
    public class UtcpHttpServer : IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };

        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private readonly RequestRouter _router = new();

        public bool IsRunning { get; private set; }
        public int Port { get; private set; }
        public RequestRouter Router => _router;

        public event Action<string> OnLog;
        public event Action<string> OnError;

        public int Start(string address, int port)
        {
            if (IsRunning) return Port;

            Port = port <= 0 ? FindFreePort() : port;
            _cts = new CancellationTokenSource();

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{address}:{Port}/");
            _listener.Start();
            IsRunning = true;

            Log($"Server started on port {Port}");
            ListenAsync(_cts.Token).Forget();

            return Port;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _listener = null;
            IsRunning = false;

            Log("Server stopped");
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }

        private int FindFreePort()
        {
            var tcp = new TcpListener(IPAddress.Loopback, 0);
            tcp.Start();
            int port = ((IPEndPoint)tcp.LocalEndpoint).Port;
            tcp.Stop();
            return port;
        }

        private async UniTaskVoid ListenAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _listener?.IsListening == true)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    if (!ct.IsCancellationRequested)
                        HandleRequestAsync(context).Forget();
                }
                catch (Exception) when (ct.IsCancellationRequested) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { LogError($"Accept error: {ex.Message}"); }
            }
        }

        private async UniTaskVoid HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            Log($"{request.HttpMethod} {request.Url.PathAndQuery}");

            try
            {
                // CORS
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                }
                else
                {
                    var result = await _router.RouteRequest(context);
                    response.StatusCode = result.StatusCode;
                    response.ContentType = result.ContentType;

                    if (result.Data != null)
                    {
                        var json = JsonConvert.SerializeObject(result.Data, JsonSettings);
                        var buffer = Encoding.UTF8.GetBytes(json);
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Request error: {ex.Message}");
                response.StatusCode = 500;
            }
            finally
            {
                try { response.Close(); } catch { }
            }
        }

        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        private void LogError(string message)
        {
            OnError?.Invoke(message);
        }
    }
}
