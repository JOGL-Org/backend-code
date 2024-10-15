using Jogl.Server.GoogleAuth.DTO;

namespace Jogl.Server.GoogleAuth
{
    public interface IGoogleFacade
    {
        Task<UserInfo> GetUserInfoAsync(string accessToken);
    }
}