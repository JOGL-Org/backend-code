using Jogl.Server.DB.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Jogl.Server.API.Middleware
{
    public class ContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ContextMiddleware> _logger;

        public ContextMiddleware( RequestDelegate next, IWebHostEnvironment environment, ILogger<ContextMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IOperationContext opContext)
        {
            try
            {
                var authorization = context.Request.Headers.Authorization.ToString();
                if (string.IsNullOrEmpty(authorization))
                    return;

                var token = new JwtSecurityToken(authorization.Substring(7));
                var claim = token.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Sid);
                if (claim == null)
                    return;

                opContext.UserId = claim.Value;
            }
            catch (Exception)
            {
                //invalid token, op context not initialized
            }
            finally
            {
                await _next(context);
            }
        }
    }
}
