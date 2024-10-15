using Microsoft.Extensions.Configuration;
using RestSharp;
using Jogl.Server.GoogleAuth.DTO;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.GoogleAuth
{
    public class GoogleFacade : IGoogleFacade
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleFacade> _logger;

        public GoogleFacade(IConfiguration configuration, ILogger<GoogleFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserInfo> GetUserInfoAsync(string accessToken)
        {

            try
            {
                var client = new RestClient($"{_configuration["GoogleAuth:InfoURL"]}");
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