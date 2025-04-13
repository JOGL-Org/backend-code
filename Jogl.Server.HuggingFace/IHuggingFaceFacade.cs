using Jogl.Server.HuggingFace.DTO;

namespace Jogl.Server.HuggingFace
{
    public interface IHuggingFaceFacade
    {
        Task<string> GetAccessTokenAsync(string authorizationCode);
        Task<User> GetUserInfoAsync(string accessToken);
        Task<List<Discussion>> ListPRsAsync(string repo);
        Task<Repo> GetRepoAsync(string repo);
        Task<List<Repo>> GetReposAsync(string accessToken);
    }
}