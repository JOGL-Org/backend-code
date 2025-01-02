using Microsoft.Extensions.Configuration;
using RestSharp;
using Microsoft.Extensions.Logging;
using Jogl.Server.Lix.DTO;
using System.Text.Json;

namespace Jogl.Server.Lix
{
    public class LixFacade : ILixFacade
    {
        private readonly IConfiguration _configuration;

        public LixFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly ILogger<LixFacade> _logger;

        public LixFacade(IConfiguration configuration, ILogger<LixFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Profile> GetProfileAsync(string linkedInUrl)
        {
            try
            {
                var client = new RestClient($"{_configuration["Lix:ApiURL"]}");
                var request = new RestRequest("person");
                request.AddParameter("profile_link", linkedInUrl, ParameterType.QueryString, true);
                request.AddHeader("Authorization", _configuration["Lix:ApiKey"]);

                var json = File.ReadAllText("bin/Debug/net8.0/data.txt");
                return JsonSerializer.Deserialize<Profile>(json);
                //var response =await client.ExecuteGetAsync<Profile>(request);
                //return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }
    }

}