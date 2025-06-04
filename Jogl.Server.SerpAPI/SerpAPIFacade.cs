using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Text.Json.Nodes;

namespace Jogl.Server.SerpAPI
{
    public class SerpAPIFacade : ISerpAPIFacade
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<SerpAPIFacade> _logger;

        public SerpAPIFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SerpAPIFacade(IConfiguration configuration, IMemoryCache memoryCache, ILogger<SerpAPIFacade> logger)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<string> GetProfileAsync(string firstName, string lastName, string affil)
        {
            var key = $"{firstName} {lastName} {affil}";
            if (!_memoryCache.TryGetValue<string>(key, out var profileData))
            {
                profileData = await GetProfileURLFromSerpAPIAsync(firstName, lastName, affil);
                _memoryCache.Set(key, profileData);
            }

            return profileData;
        }

        public async Task<string> GetProfileURLFromSerpAPIAsync(string firstName, string lastName, string affil)
        {
            try
            {
                var searchTerm = $"{firstName} {lastName} {affil}";
                var client = new RestClient("https://serpapi.com");
                var request = new RestRequest($"/search");
                request.AddQueryParameter("api_key", "05fe9d3608db6290aa05d4830e672f5dc39bc699a710b16dbb1f71c08a024e77");
                request.AddQueryParameter("q", searchTerm);
                request.AddQueryParameter("engine", "google");
                request.AddQueryParameter("hl", "en");
                request.AddQueryParameter("gl", "us");

                var response = await client.ExecuteAsync(request);
                var node = JsonNode.Parse(response.Content);
                return node["organic_results"].AsArray()[0]["link"].AsValue().ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }
    }

}