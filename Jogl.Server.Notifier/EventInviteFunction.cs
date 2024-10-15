using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public class EventInviteFunction
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPushNotificationTokenRepository _pushNotificationTokenRepository;
        private readonly IEmailService _emailService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IUrlService _urlService;
        private readonly ILogger<EventInviteFunction> _logger;

        public EventInviteFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IEventRepository eventRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<EventInviteFunction> logger)
        {
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _pushNotificationTokenRepository = pushNotificationTokenRepository;
            _emailService = emailService;
            _pushNotificationService = pushNotificationService;
            _urlService = urlService;
            _logger = logger;
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

            //await _emailService.SendEmailAsync(invitee.Email, EmailTemplate.UserInvitedToContainer, new
            //{
            //    NAME = inviter.FeedTitle,
            //    EVENT_URL = _urlService.GetUrl(ev),
            //    EVENT_NAME = ev.Title,
            //    CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
            //    CONTAINER_URL = _urlService.GetUrl(communityEntity),
            //    CONTAINER_NAME = communityEntity.FeedTitle,
            //    CTA_URL = _urlService.GetUrl(communityEntity),
            //});

            if (invitee.NotificationSettings?.EventInvitationJogl == true)
            {
                var pushTokens = _pushNotificationTokenRepository.List(t => t.UserId == invitation.UserId && !t.Deleted);
                //var pushData = pushTokens
                //  .ToDictionary(u => u.Token, u => (object)new
                //  {
                //      EVENT_URL = _urlService.GetUrl(ev),
                //      EVENT_NAME = ev.Title,
                //      CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
                //      CONTAINER_URL = _urlService.GetUrl(communityEntity),
                //      CONTAINER_NAME = communityEntity.FeedTitle,
                //  });

                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(), $"You are invited to an event", $"{inviter.FullName} invited you to {ev.Title}", _urlService.GetUrl(ev));
            }
            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
