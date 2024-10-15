using System.Text.Json.Nodes;

namespace Jogl.Server.API.Middleware
{
    public class JObjectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JObjectMiddleware> _logger;

        public JObjectMiddleware(RequestDelegate next, ILogger<JObjectMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(bodyAsText))
            {
                try
                {
                    context.Items["jo"] = JsonNode.Parse(bodyAsText);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            await _next.Invoke(context);
        }
    }
}
