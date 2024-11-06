using Jogl.Server.Data;

namespace Jogl.Server.Notifications
{
    public interface INotificationFacade
    {
        Task NotifyCreatedAsync(ContentEntity contentEntity);
        Task NotifyCreatedAsync(Comment comment);
        Task NotifyCreatedAsync(Event ev);
        Task NotifyCreatedAsync(Need need);
        Task NotifyCreatedAsync(Document doc);
        Task NotifyUpdatedAsync(Document doc);
        Task NotifyCreatedAsync(Notification notification);
        Task NotifyCreatedAsync(IEnumerable<Notification> notifications);
        Task NotifyAddedAsync(Paper paper);
        Task NotifyLoadedAsync(IEnumerable<Publication> publications);
        Task NotifyLoadedAsync(Publication publication);
        Task NotifyInvitedAsync(Invitation invitation);
        Task NotifyInvitedAsync(List<Invitation> invitations);
        Task NotifyInvitedAsync(EventAttendance invitation);
        Task NotifyInvitedAsync(List<EventAttendance> invitations);
    }
}