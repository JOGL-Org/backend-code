using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Localization;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Invite
{
    public class EventInviteFunction : NotificationFunctionBase
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IEventRepository _eventRepository;

        public EventInviteFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IEventRepository eventRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _eventRepository = eventRepository;
        }

        [Function(nameof(EventInviteFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger("event-invitation-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var invitation = JsonSerializer.Deserialize<EventAttendance>(message.Body.ToString());
            var ev = _eventRepository.Get(invitation.EventId);
            if (string.IsNullOrEmpty(invitation.UserId))
                return;

            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId);
            var inviter = _userRepository.Get(invitation.CreatedByUserId);
            var invitee = _userRepository.Get(invitation.UserId);

            if (invitee.NotificationSettings?.EventInvitationJogl == true)
            {
                var pushTokens = _pushNotificationTokenRepository.List(t => t.UserId == invitation.UserId && !t.Deleted);
                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(),
                       _localizationService.GetString("templates.push.eventInvite.title", invitee.Language),
                       _localizationService.GetString("templates.push.eventInvite.body", invitee.Language, inviter.FullName, ev.Title),
                       _urlService.GetUrl(ev));

            }
            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
