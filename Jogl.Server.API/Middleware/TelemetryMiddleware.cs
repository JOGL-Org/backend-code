using Microsoft.ApplicationInsights.DataContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class TelemetryMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
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

            var requestTelemetry = context.Features.Get<RequestTelemetry>();
            requestTelemetry?.Properties.Add("JOGL.UserID", claim.Value);

        }
        catch (Exception)
        {
            //invalid token, no user id will be present
        }
        finally
        {
            await next(context);
        }
    }
}