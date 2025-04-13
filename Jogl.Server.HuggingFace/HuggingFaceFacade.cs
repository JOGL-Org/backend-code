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

        public async Task<string> GetAccessTokenAsync(string authorizationCode)
        {
            try
            {
                var client = new RestClient($"https://huggingface.co/oauth/token");
                var request = new RestRequest("/");
                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("code", authorizationCode);
                request.AddParameter("client_id", _configuration["HuggingFace:ClientId"]);
                request.AddParameter("client_secret", _configuration["HuggingFace:ClientSecret"]);
                request.AddParameter("redirect_uri", _configuration["HuggingFace:RedirectURL"]);

                var response = await client.ExecutePostAsync<TokenInfo>(request);
                return response.Data?.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<User> GetUserInfoAsync(string accessToken)
        {
            var client = new RestClient($"https://huggingface.co/api/");
            client.AddDefaultHeader("Authorization", $"Bearer {accessToken}");

            var userRequest = new RestRequest($"whoami-v2");
            var userResponse = await client.ExecuteGetAsync<User>(userRequest);
            if (!userResponse.IsSuccessStatusCode)
                return null;

            return userResponse.Data;
        }

        public async Task<Repo> GetRepoAsync(string repo)
        {
            var client = new RestClient($"https://huggingface.co/api/");

            if (repo.StartsWith("spaces/"))
            {
                var spaceRequest = new RestRequest($"{repo}");
                var spaceResponse = await client.ExecuteGetAsync<SpaceRepo>(spaceRequest);
                if (!spaceResponse.IsSuccessStatusCode)
                    return null;

                return spaceResponse.Data;
            }

            if (repo.StartsWith("datasets/"))
            {
                var datasetRequest = new RestRequest($"{repo}");
                var datasetResponse = await client.ExecuteGetAsync<DataSetRepo>(datasetRequest);
                if (!datasetResponse.IsSuccessStatusCode)
                    return null;

                return datasetResponse.Data;
            }

            var modelRequest = new RestRequest($"models/{repo}");
            var modelResponse = await client.ExecuteGetAsync<ModelRepo>(modelRequest);
            if (!modelResponse.IsSuccessStatusCode)
                return null;

            return modelResponse.Data;
        }

        public async Task<string> GetReadmeAsync(string repo, string accessToken)
        {
            var client = new RestClient($"https://huggingface.co/");

            var modelRequest = new RestRequest($"{repo}/raw/main/README.md");
            var modelResponse = await client.ExecuteGetAsync<ModelRepo>(modelRequest);
            if (!modelResponse.IsSuccessStatusCode)
                return null;

            return modelResponse.Content;
        }


        public async Task<List<Repo>> GetReposAsync(string accessToken)
        {
            var client = new RestClient($"https://huggingface.co/api/");
            client.AddDefaultHeader("Authorization", $"Bearer {accessToken}");

            var userRequest = new RestRequest($"whoami-v2");
            var userResponse = await client.ExecuteGetAsync<User>(userRequest);

            var modelRequest = new RestRequest($"models");
            modelRequest.AddQueryParameter("author", userResponse.Data.Name);
            var datasetRequest = new RestRequest($"datasets");
            datasetRequest.AddQueryParameter("author", userResponse.Data.Name);
            var spaceRequest = new RestRequest($"spaces");
            spaceRequest.AddQueryParameter("author", userResponse.Data.Name);

            var res = new List<Repo>();

            var modelResponse = await client.ExecuteGetAsync<List<ModelRepo>>(modelRequest);
            var datasetResponse = await client.ExecuteGetAsync<List<DataSetRepo>>(datasetRequest);
            var spaceResponse = await client.ExecuteGetAsync<List<SpaceRepo>>(spaceRequest);

            if (modelResponse.IsSuccessStatusCode)
                res.AddRange(modelResponse.Data);

            if (datasetResponse.IsSuccessStatusCode)
                res.AddRange(datasetResponse.Data);

            if (spaceResponse.IsSuccessStatusCode)
                res.AddRange(spaceResponse.Data);

            return res;
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