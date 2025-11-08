using Azure.Core;
using Jogl.Server.DB;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class ExternalIdResult : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    public const string API_KEY_HEADER_NAME = "X-Api-Key";

    public ExternalIdResult(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository)
        : base(options, logger, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var apiKeyData = _apiKeyRepository.Get(ak => ak.Key == providedApiKey);
        if (apiKeyData == null)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        Context.Items["NodeId"] = apiKeyData.NodeId;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, apiKeyData.Id.ToString()),
            new Claim("ApiKeyId", apiKeyData.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}