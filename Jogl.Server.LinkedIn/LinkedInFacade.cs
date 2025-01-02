using Microsoft.Extensions.Configuration;
using RestSharp;
using Jogl.Server.LinkedIn.DTO;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.LinkedIn
{
    public class LinkedInFacade : ILinkedInFacade
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LinkedInFacade> _logger;

        public LinkedInFacade(IConfiguration configuration, ILogger<LinkedInFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(string authorizationCode)
        {
            try
            {
                var client = new RestClient($"{_configuration["LinkedIn:AccessTokenURL"]}");
                var request = new RestRequest("/");
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&code={authorizationCode}&client_id={_configuration["LinkedIn:ClientId"]}&client_secret={_configuration["LinkedIn:ClientSecret"]}&redirect_uri={_configuration["LinkedIn:RedirectURL"]}", ParameterType.RequestBody);

                var response = await client.ExecuteGetAsync<TokenInfo>(request);
                return response.Data?.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                var client = new RestClient($"{_configuration["LinkedIn:InfoURL"]}");
                var request = new RestRequest("/");
                request.AddHeader("Authorization", $"Bearer {accessToken}");

                var response = await client.ExecuteGetAsync<UserInfo>(request);
                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

    }

}