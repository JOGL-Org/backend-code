using Microsoft.Extensions.Configuration;
using RestSharp;
using Microsoft.Extensions.Logging;
using Jogl.Server.HuggingFace.DTO;

namespace Jogl.Server.HuggingFace
{
    public class HuggingFaceFacade : IHuggingFaceFacade
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HuggingFaceFacade> _logger;

        public HuggingFaceFacade(IConfiguration configuration, ILogger<HuggingFaceFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Repo> GetRepoAsync(string repo)
        {
            var client = new RestClient($"https://huggingface.co/api/");
            var request = new RestRequest($"{repo}");

            var response = await client.ExecuteGetAsync<Repo>(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return response.Data;
        }

        public async Task<List<Discussion>> ListPRsAsync(string repo)
        {
            try
            {
                var client = new RestClient($"https://huggingface.co/api/");
                var request = new RestRequest($"{repo}/discussions?status=open&type=pull_request");
                var response = await client.ExecuteGetAsync<DiscussionResponse>(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                return response.Data.Discussions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new List<Discussion>();
            }
        }
    }
}