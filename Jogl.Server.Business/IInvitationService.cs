using Jogl.Server.Business.DTO;
using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IInvitationService
    {
        Task<string> CreateAsync(Invitation invitation);
        Task<List<OperationResult<string>>> CreateMultipleAsync(IEnumerable<Invitation> invitations, string redirectUrl);
        Invitation Get(string invitationId);
        Invitation Get(string invitationId, string userId);
        Invitation GetForUserAndEntity(string userId, string entityId);
        Invitation GetForEmailAndEntity(string email, string entityId);
        List<Invitation> List(string userId);
        List<Invitation> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, bool loadDetails = false);
        List<Invitation> ListForUser(string userId, InvitationType? type = null);
        Task AcceptAsync(Invitation invitation);
        Task RejectAsync(Invitation invitation);
        Task ResendAsync(Invitation invitation);
        InvitationKey GetInvitationKey(string entityId, string key);
        Task<string> GetInvitationKeyForEntityAsync(string entityId);
    }
}