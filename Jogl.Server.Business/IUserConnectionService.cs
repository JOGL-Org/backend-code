using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IUserConnectionService
    {
        Task<string> InviteAsync(UserConnection connection);
        List<User> ListConnectedUsers(string userId);
        UserConnection Get(string userId1, string userId2);
        Task AcceptInvitationAsync(UserConnection connection);
        Task RejectInvitationAsync(UserConnection connection);
    }
}