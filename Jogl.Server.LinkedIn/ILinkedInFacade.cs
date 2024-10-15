using Jogl.Server.LinkedIn.DTO;

namespace Jogl.Server.LinkedIn
{
    public interface ILinkedInFacade
    {
        Task<string> GetAccessTokenAsync(string authorizationCode);
        Task<UserInfo> GetUserInfoAsync(string accessToken);
    }
}