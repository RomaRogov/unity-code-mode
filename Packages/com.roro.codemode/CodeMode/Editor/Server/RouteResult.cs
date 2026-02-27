namespace CodeMode.Editor.Server
{
    public class RouteResult
    {
        public int StatusCode { get; set; } = 200;
        public string ContentType { get; set; } = "application/json";
        public object Data { get; set; }

        public static RouteResult Ok(object data) => new() { Data = data };

        public static RouteResult BadRequest(string message) => new()
        {
            StatusCode = 400,
            Data = new { error = message, code = 400 }
        };

        public static RouteResult NotFound(string message) => new()
        {
            StatusCode = 404,
            Data = new { error = message, code = 404 }
        };

        public static RouteResult InternalError(string message) => new()
        {
            StatusCode = 500,
            Data = new { error = message, code = 500 }
        };
    }
}
