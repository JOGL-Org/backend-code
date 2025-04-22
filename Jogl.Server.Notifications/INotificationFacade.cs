using Jogl.Server.Data;

namespace Jogl.Server.Notifications
{
    public interface INotificationFacade
    {
        Task NotifyCreatedAsync(ContentEntity contentEntity);
        Task NotifyCreatedAsync(Comment comment);
        Task NotifyCreatedAsync(Event ev);
        Task NotifyCreatedAsync(Need need);
        Task NotifyUpdatedAsync(Need need);
        Task NotifyCreatedAsync(Document doc);
        Task NotifyUpdatedAsync(Document doc);
        Task NotifyCreatedAsync(Paper paper);
        Task NotifyUpdatedAsync(Paper paper);
        Task NotifyLoadedAsync(IEnumerable<Publication> publications);
        Task NotifyLoadedAsync(Publication publication);
        Task NotifyInvitedAsync(Invitation invitation);
        Task NotifyInvitedAsync(List<Invitation> invitations);
        Task NotifyInvitedAsync(EventAttendance invitation);
        Task NotifyInvitedAsync(List<EventAttendance> invitations);
        Task NotifyOnboardingCompletedAsync(User user);
        Task NotifyAsync<T>(string queueOrTopicName, T data);
    }
}