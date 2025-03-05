using Microsoft.Extensions.Configuration;
using RestSharp;
using Microsoft.Extensions.Logging;
using Jogl.Server.Lix.DTO;
using Microsoft.Extensions.Caching.Memory;

namespace Jogl.Server.Lix
{
    public class LixFacade : ILixFacade
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<LixFacade> _logger;

        public LixFacade(IConfiguration configuration, IMemoryCache memoryCache, ILogger<LixFacade> logger)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<Profile> GetProfileAsync(string linkedInUrl)
        {
            if (!_memoryCache.TryGetValue<Profile>(linkedInUrl, out var profileData))
            {
                profileData = await GetProfileFromLixAsync(linkedInUrl);
                _memoryCache.Set(linkedInUrl, profileData);
            }

            return profileData;
        }

        private async Task<Profile> GetProfileFromLixAsync(string linkedInUrl)
        {
            try
            {
                var client = new RestClient($"{_configuration["Lix:ApiURL"]}");
                var request = new RestRequest("person");
                request.AddParameter("profile_link", linkedInUrl, ParameterType.QueryString, true);
                request.AddHeader("Authorization", _configuration["Lix:ApiKey"]);

                var response = await client.ExecuteGetAsync<Profile>(request);
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