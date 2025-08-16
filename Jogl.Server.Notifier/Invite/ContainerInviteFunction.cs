using System.Net;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.Localization;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Jogl.Server.Verification;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Invite
{
    public class ContainerInviteFunction : NotificationFunctionBase
    {
        private readonly ICommunityEntityService _communityEntityService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserVerificationCodeRepository _userVerificationRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserVerificationService _userVerificationService;

        public ContainerInviteFunction(ICommunityEntityService communityEntityService, IFeedEntityService feedEntityService, IMembershipRepository membershipRepository, IUserVerificationCodeRepository verificationCodeRepository, IUserVerificationService userVerificationService, IConfiguration configuration, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILocalizationService localizationService, ILogger<NotificationFunctionBase> logger) : base(userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, localizationService, logger)
        {
            _communityEntityService = communityEntityService;
            _feedEntityService = feedEntityService;
            _membershipRepository = membershipRepository;
            _userVerificationRepository = verificationCodeRepository;
            _userVerificationService = userVerificationService;
            _configuration = configuration;
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
                await _emailService.SendEmailAsync(invitee.Email, EmailTemplate.InvitationWithUser, new
                {
                    NAME = inviter.FeedTitle,
                    CONTAINER_TYPE = _localizationService.GetString(communityEntity.FeedType.ToString(), invitee.Language),
                    CONTAINER_NAME = communityEntity.FeedTitle,
                    CTA_URL = _urlService.GetUrl("actions"),
                    LANGUAGE = invitee.Language
                }, fromName: inviter.FirstName);
            }

            if (invitee.NotificationSettings?.ContainerInvitationJogl == true)
            {
                var pushTokens = _pushNotificationTokenRepository.List(t => t.UserId == invitation.InviteeUserId && !t.Deleted);
                await _pushNotificationService.PushAsync(pushTokens.Select(t => t.Token).ToList(),
                    _localizationService.GetString("templates.push.containerInvite.title", invitee.Language, communityEntity.FeedType),
                    _localizationService.GetString("templates.push.containerInvite.body", invitee.Language, inviter.FullName, communityEntity.FeedType),
                    _urlService.GetUrl("actions"));
            }
        }

        private async Task NotifyEmail(Invitation invitation)
        {
            var redirectUrl = $"{_configuration["App:URL"]}/signup";
            var communityEntity = _communityEntityService.Get(invitation.CommunityEntityId);
            var inviterUser = _userRepository.Get(invitation.CreatedByUserId);
            var verificationCode = await _userVerificationService.CreateAsync(new User { Email = invitation.InviteeEmail }, VerificationAction.Verify, null, false);
            await _emailService.SendEmailAsync(invitation.InviteeEmail, EmailTemplate.InvitationWithEmail, new
            {
                NAME = inviterUser.FeedTitle,
                CONTAINER_TYPE = _localizationService.GetString(communityEntity.FeedType.ToString()),
                CONTAINER_NAME = communityEntity.FeedTitle,
                CTA_URL = redirectUrl + $"?email={WebUtility.UrlEncode(invitation.InviteeEmail)}&verification_code={verificationCode}",
            }, fromName: inviterUser.FirstName);
        }
    }
}
