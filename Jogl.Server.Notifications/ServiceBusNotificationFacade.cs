using Azure;
using Jogl.Server.Data;
using Jogl.Server.ServiceBus;

namespace Jogl.Server.Notifications
{
    public class ServiceBusNotificationFacade : INotificationFacade
    {
        private readonly IServiceBusProxy _serviceBus;
        public ServiceBusNotificationFacade(IServiceBusProxy serviceBus)
        {
            _serviceBus = serviceBus;
        }

        public async Task NotifyCreatedAsync(ContentEntity contentEntity)
        {
            await _serviceBus.SendAsync(contentEntity, "content-entity-created");
        }

        public async Task NotifyCreatedAsync(Comment comment)
        {
            await _serviceBus.SendAsync(comment, "comment-created");
        }

        public async Task NotifyCreatedAsync(Event ev)
        {
            await _serviceBus.SendAsync(ev, "event-created");
        }

        public async Task NotifyCreatedAsync(Need need)
        {
            await _serviceBus.SendAsync(need, "need-created");
        }

        public async Task NotifyUpdatedAsync(Need need)
        {
            await _serviceBus.SendAsync(need, "need-updated");
        }

        public async Task NotifyCreatedAsync(Document doc)
        {
            await _serviceBus.SendAsync(doc, "document-created");
        }

        public async Task NotifyUpdatedAsync(Document doc)
        {
            await _serviceBus.SendAsync(doc, "document-updated");
        }

        public async Task NotifyCreatedAsync(Paper paper)
        {
            await _serviceBus.SendAsync(paper, "paper-created");
        }

        public async Task NotifyUpdatedAsync(Paper paper)
        {
            await _serviceBus.SendAsync(paper, "paper-updated");
        }

        public async Task NotifyLoadedAsync(IEnumerable<Publication> publications)
        {
            await _serviceBus.SendAsync(publications, "publication-loaded");
        }

        public async Task NotifyLoadedAsync(Publication publication)
        {
            await _serviceBus.SendAsync(publication, "publication-loaded");
        }

        public async Task NotifyInvitedAsync(Invitation invitation)
        {
            await _serviceBus.SendAsync(invitation, "invitation-created");
        }

        public async Task NotifyInvitedAsync(List<Invitation> invitations)
        {
            foreach (var invitation in invitations)
            {
                await NotifyInvitedAsync(invitation);
            }
        }

        public async Task NotifyInvitedAsync(EventAttendance invitation)
        {
            await _serviceBus.SendAsync(invitation, "event-invitation-created");
        }

        public async Task NotifyInvitedAsync(List<EventAttendance> invitations)
        {
            foreach (var invitation in invitations)
            {
                await NotifyInvitedAsync(invitation);
            }
        }

        public async Task NotifyOnboardingCompletedAsync(User user)
        {
            await _serviceBus.SendAsync(user, "onboarding-completed");
        }
    }
}