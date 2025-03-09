using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface INotificationService
    {
        bool HasPendingSince(string userId, DateTime? dateTimeUTC);
        ListPage<Notification> ListSince(string userId, DateTime? dateTimeUTC, int page, int pageSize);
        Notification Get(string notificationId);
        Task UpdateAsync(Notification notification);
        Task NotifyRequestAcceptedAsync(Invitation invitation);
        Task NotifyAccessLevelChangedAsync(Membership membership);
        Task NotifyUserFollowedAsync(UserFollowing following);
        Task NotifyResourceCreatedAsync(Resource resource);
        Task NotifyNeedCreatedAsync(Need need);
        Task NotifyCommunityEntityJoinedAsync(Relation relation);
        Task NotifyMemberJoinedAsync(Membership membership);
        Task NotifyInviteCreatedAsync(Invitation invitation, User user);
        Task NotifyInvitesCreatedAsync(IEnumerable<Invitation> invitations);
        Task NotifyInviteWithdrawAsync(Invitation invitation);
        Task NotifyInvitesWithdrawAsync(IEnumerable<Invitation> invitations);
        Task NotifyEventInviteCreatedAsync(Event ev, CommunityEntity communityEntity, User user, IEnumerable<EventAttendance> invitations);
        Task NotifyEventInviteUpdatedAsync(Event ev, CommunityEntity communityEntity, User user, IEnumerable<EventAttendance> invitations);
        Task NotifyEventInviteWithdrawAsync(EventAttendance invitation);
        Task NotifyEventInvitesWithdrawAsync(IEnumerable<EventAttendance> invitations);
        Task NotifyRequestCreatedAsync(Invitation invitation);
        Task NotifyRequestsCreatedAsync(IEnumerable<Invitation> invitations);
        Task NotifyRequestCreatedWithdrawAsync(Invitation invitation);
        Task NotifyCommunityEntityInviteCreatedAsync(CommunityEntityInvitation invitation);
        Task NotifyCommunityEntityInviteCreatedWithdrawAsync(CommunityEntityInvitation invitation);


    }
}