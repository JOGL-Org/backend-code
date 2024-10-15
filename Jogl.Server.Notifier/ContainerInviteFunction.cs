using System.Net;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpCompress.Writers;

namespace Jogl.Server.Notifier
{
    public class ContainerInviteFunction
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPushNotificationTokenRepository _pushNotificationTokenRepository;
        private readonly IUserVerificationCodeRepository _userVerificationRepository;
        private readonly IEmailService _emailService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IConfiguration _configuration;
        private readonly IUrlService _urlService;
        private readonly IUserVerificationService _userVerificationService;
        private readonly ILogger<ContainerInviteFunction> _logger;

        public ContainerInviteFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IUserVerificationCodeRepository userVerificationRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, IUserVerificationService userVerificationService, IConfiguration configuration, ILogger<ContainerInviteFunction> logger)
        {
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _pushNotificationTokenRepository = pushNotificationTokenRepository;
            _userVerificationRepository = userVerificationRepository;
            _emailService = emailService;
            _pushNotificationService = pushNotificationService;
            _configuration = configuration;
            _urlService = urlService;
            _userVerificationService = userVerificationService;
            _logger = logger;
        }

        [Function(nameof(ContainerInviteFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger("invitation-created", "notifications", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var invitation = JsonSerializer.Deserialize<Invitation>(message.Body.ToString());
            if (string.IsNullOrEmpty(invitation.InviteeUserId))
                await NotifyEmail(invitation);
            else
                await NotifyUser(invitation);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }

        private async Task NotifyUser(Invitation invitation)
        {
            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId);
            var inviter = _userRepository.Get(invitation.CreatedByUserId);
            var invitee = _userRepository.Get(invitation.InviteeUserId);

            if (invitee.NotificationSettings?.ContainerInvitationEmail == true)
            {
                await _emailService.SendEmailAsync(invitee.Email, EmailTemplate.UserInvitedToContainer, new
                {
                    NAME = inviter.FeedTitle,
                    CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
                    CONTAINER_URL = _urlService.GetUrl(communityEntity),
                    CONTAINER_NAME = communityEntity.FeedTitle,
                    CTA_URL = _urlService.GetUrl("actions"),
                }, fromName: inviter.FirstName);
            }

            if (invitee.NotificationSettings?.ContainerInvitationJogl == true)
            {
                var pushTokens = _pushNotificationTokenRepository.List(t => t.UserId == invitation.InviteeUserId && !t.Deleted);
                var pushData = pushTokens
                  .ToDictionary(u => u.Token, u => (object)new
                  {
                      CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
                      CONTAINER_URL = _urlService.GetUrl(communityEntity),
                      CONTAINER_NAME = communityEntity.FeedTitle,
                  });

                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(), $"Invitation to a {_feedEntityService.GetPrintName(communityEntity.FeedType)}", $"{inviter.FullName} invited you to join their {_feedEntityService.GetPrintName(communityEntity.FeedType)}", _urlService.GetUrl("actions"));
            }
        }

        private async Task NotifyEmail(Invitation invitation)
        {
            var redirectUrl = $"{_configuration["App:URL"]}/signup";
            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId);
            var inviterUser = _userRepository.Get(invitation.CreatedByUserId);
            var verificationCode = await _userVerificationService.CreateAsync(new User { Email = invitation.InviteeEmail }, VerificationAction.Verify, false);
            await _emailService.SendEmailAsync(invitation.InviteeEmail, EmailTemplate.InvitationWithEmail, new
            {
                NAME = inviterUser.FeedTitle,
                CONTAINER_TYPE = _feedEntityService.GetPrintName(communityEntity.FeedType),
                CONTAINER_NAME = communityEntity.FeedTitle,
                CTA_URL = redirectUrl + $"?email={WebUtility.UrlEncode(invitation.InviteeEmail)}&verification_code={verificationCode}",
                invitor = inviterUser.FeedTitle, //TODO delete after release
                url = redirectUrl + $"?email={WebUtility.UrlEncode(invitation.InviteeEmail)}&verification_code={verificationCode}",//TODO delete after release
                access_level = invitation.AccessLevel.ToString(),//TODO delete after release
                entity_type = _communityEntityService.GetPrintName(invitation.CommunityEntityType),//TODO delete after release
                entity_name = communityEntity.Title//TODO delete after release
            }, fromName: inviterUser.FirstName);
        }
    }
}
