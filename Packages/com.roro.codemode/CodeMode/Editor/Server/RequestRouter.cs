using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeMode.Editor.Server
{
    /// <summary>
    /// Simple router for tool requests
    /// Collects data from query and body
    /// </summary>
    public class RequestRouter
    {
        private readonly List<Route> _routes = new List<Route>();

        public void AddRoute(string method, string pattern, Func<RequestContext, UniTask<RouteResult>> handler)
        {
            _routes.Add(new Route
            {
                Method = method.ToUpperInvariant(),
                Regex = PatternToRegex(pattern),
                Handler = handler
            });
        }

        public void Get(string pattern, Func<RequestContext, UniTask<RouteResult>> handler)
        {
            AddRoute("GET", pattern, handler);
        }

        public void Post(string pattern, Func<RequestContext, UniTask<RouteResult>> handler)
        {
            AddRoute("POST", pattern, handler);
        }

        public void Put(string pattern, Func<RequestContext, UniTask<RouteResult>> handler)
        {
            AddRoute("PUT", pattern, handler);
        }

        public void Delete(string pattern, Func<RequestContext, UniTask<RouteResult>> handler)
        {
            AddRoute("DELETE", pattern, handler);
        }

        public async UniTask<RouteResult> RouteRequest(HttpListenerContext httpContext)
        {
            var request = httpContext.Request;
            var method = request.HttpMethod.ToUpperInvariant();
            var path = request.Url.AbsolutePath;

            // Find matching route
            foreach (var route in _routes)
            {
                if (route.Method != method && route.Method != "*")
                    continue;

                var match = route.Regex.Match(path);
                if (!match.Success)
                    continue;

                // Extract path parameters
                var pathParams = new Dictionary<string, string>();
                var groupNames = route.Regex.GetGroupNames();
                foreach (var name in groupNames)
                {
                    if (name != "0" && match.Groups[name].Success)
                    {
                        pathParams[name] = match.Groups[name].Value;
                    }
                }

                // Parse query parameters (with qs.parse-style bracket notation support)
                var queryParams = QueryStringParser.Parse(request.Url.Query);

                // Read body
                string body = null;
                if (request.HasEntityBody)
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        body = await reader.ReadToEndAsync();
                    }
                }

                var context = new RequestContext
                {
                    HttpContext = httpContext,
                    Method = method,
                    Path = path,
                    PathParams = pathParams,
                    QueryParams = queryParams,
                    Body = body,
                    Headers = new Dictionary<string, string>()
                };

                // Copy headers
                foreach (string key in request.Headers.AllKeys)
                {
                    context.Headers[key] = request.Headers[key];
                }

                try
                {
                    return await route.Handler(context);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UTCP] Route handler error: {ex}");
                    return RouteResult.InternalError(ex.Message);
                }
            }

            // No route matched - check if path exists but method is wrong
            foreach (var route in _routes)
            {
                var match = route.Regex.Match(path);
                if (match.Success)
                {
                    return RouteResult.BadRequest($"Method {method} not allowed for {path}");
                }
            }

            return RouteResult.NotFound($"No route found for {method} {path}");
        }

        private Regex PatternToRegex(string pattern)
        {
            // Convert route pattern to regex
            // /tools/{toolName} -> /tools/(?<toolName>[^/]+)
            var regexPattern = Regex.Replace(pattern, @"\{(\w+)\}", @"(?<$1>[^/]+)");
            regexPattern = "^" + regexPattern + "$";
            return new Regex(regexPattern, RegexOptions.Compiled);
        }

        private class Route
        {
            public string Method;
            public Regex Regex;
            public Func<RequestContext, UniTask<RouteResult>> Handler;
        }
    }

    public class RequestContext
    {
        public HttpListenerContext HttpContext { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> PathParams { get; set; }
        public Dictionary<string, object> QueryParams { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public object GetParam(string name, object defaultValue = null)
        {
            if (PathParams != null && PathParams.TryGetValue(name, out var pathValue))
                return pathValue;

            if (QueryParams != null && QueryParams.TryGetValue(name, out var queryValue))
                return queryValue;

            return defaultValue;
        }
    }
}
