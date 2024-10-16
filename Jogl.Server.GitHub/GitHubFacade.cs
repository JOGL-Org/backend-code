using Microsoft.Extensions.Configuration;
using RestSharp;
using Microsoft.Extensions.Logging;
using Jogl.Server.GitHub.DTO;

namespace Jogl.Server.GitHub
{
    public class GitHubFacade : IGitHubFacade
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GitHubFacade> _logger;

        public GitHubFacade(IConfiguration configuration, ILogger<GitHubFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(string authorizationCode)
        {
            try
            {
                var client = new RestClient($"https://github.com/login/oauth/access_token");
                var request = new RestRequest("/");
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&code={authorizationCode}&client_id={_configuration["GitHub:ClientId"]}&client_secret={_configuration["GitHub:ClientSecret"]}&redirect_uri={_configuration["GitHub:RedirectURL"]}", ParameterType.RequestBody);

                var response = await client.ExecuteGetAsync<TokenInfo>(request);
                return response.Data?.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<Repo> GetRepoAsync(string repo, string accessToken)
        {
            try
            {
                var client = new RestClient($"https://api.github.com/");
                var request = new RestRequest($"repos/{repo}");
                request.AddHeader("Authorization", $"Bearer {accessToken}");

                var response = await client.ExecuteGetAsync<Repo>(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<List<PullRequest>> ListPRsAsync(string repo)
        {
            try
            {
                var client = new RestClient($"https://api.github.com/");
                var request = new RestRequest($"repos/{repo}/pulls?sort=created");
                //request.AddHeader("Authorization", $"Bearer {accessToken}");

                var response = await client.ExecuteGetAsync<List<PullRequest>>(request);
                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new List<PullRequest>();
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                var client = new RestClient($"{_configuration["GitHub:InfoURL"]}");
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