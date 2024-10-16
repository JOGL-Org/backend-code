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
            try
            {
                var client = new RestClient($"https://huggingface.co/api/");
                var request = new RestRequest($"models/{repo}");

                var response = await client.ExecuteGetAsync<Repo>(request);
                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<List<Discussion>> ListPRsAsync(string repo)
        {
            try
            {
                var client = new RestClient($"https://huggingface.co/api/");
                var request = new RestRequest($"models/{repo}/discussions?status=open&type=pull_request");
                //request.AddHeader("Authorization", $"Bearer {accessToken}");

                var response = await client.ExecuteGetAsync<DiscussionResponse>(request);
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