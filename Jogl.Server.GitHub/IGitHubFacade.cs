using Jogl.Server.GitHub.DTO;

namespace Jogl.Server.GitHub
{
    public interface IGitHubFacade
    {
        Task<string> GetAccessTokenAsync(string authorizationCode);
        Task<UserInfo> GetUserInfoAsync(string accessToken);
        Task<List<PullRequest>> ListPRsAsync(string repo);
        Task<Repo> GetRepoAsync(string repo, string accessToken);
    }
}