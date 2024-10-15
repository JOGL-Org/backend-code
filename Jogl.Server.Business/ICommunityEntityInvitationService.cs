using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface ICommunityEntityInvitationService
    {
        Task<string> CreateAsync(CommunityEntityInvitation invitation, string redirectUrl);
        CommunityEntityInvitation Get(string invitationId);
        CommunityEntityInvitation GetForSourceAndTarget(string sourceEntityId, string targetEntityId);
        List<CommunityEntityInvitation> ListForTarget(string currentUserId, string entityId, string search, int page, int pageSize);
        List<CommunityEntityInvitation> ListForSource(string currentUserId, string entityId, string search, int page, int pageSize);
        Task AcceptAsync(CommunityEntityInvitation invitation);
        Task RejectAsync(CommunityEntityInvitation invitation);
    }
}