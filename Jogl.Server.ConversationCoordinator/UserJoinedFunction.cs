using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Business;
using Jogl.Server.Conversation.Data;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Slack;
using Jogl.Server.URL;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class UserJoinedFunction
    {
        private readonly ISlackService _slackService;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceUserRepository _interfaceUserRepository;
        private readonly IUserService _userService;
        private readonly IUrlService _urlService;
        private readonly IMembershipService _membershipService;
        private readonly ILogger<UserJoinedFunction> _logger;

        public UserJoinedFunction(ISlackService slackService, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceUserRepository interfaceUserRepository, IUserService userService, IUrlService urlService, ILogger<UserJoinedFunction> logger)
        {
            _slackService = slackService;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceUserRepository = interfaceUserRepository;
            _userService = userService;
            _urlService = urlService;
            _logger = logger;
        }

        [Function(nameof(UserJoinedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.USER_JOINED_INTERFACE_CHANNEL, Connection = "ConnectionString", AutoCompleteMessages =true)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var userJoined = JsonSerializer.Deserialize<UserJoined>(message.Body.ToString());
            var user = userJoined.User;

            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == userJoined.ChannelExternalId);
            if (channel == null)
            {
                _logger.LogWarning("Channel not known: {0}", userJoined.ChannelExternalId);
                return;
            }

            if (string.IsNullOrEmpty(channel.Key))
            {
                _logger.LogWarning("Channel not initialized with access key {0}", channel.ExternalId);
                return;
            }

            if (string.IsNullOrEmpty(channel.NodeId))
            {
                _logger.LogWarning("Slack workspace missing a node id: {0}", channel.ExternalId);
                return;
            }

            var code = await _userService.GetOnetimeLoginCodeAsync(user.Email);
            var url = _urlService.GetOneTimeLoginLink(user.Email, code);
            var channelId = await _slackService.GetUserChannelIdAsync(channel.Key, user.ExternalId);

            var existingUser = _userService.GetForEmail(user.Email);
            if (existingUser != null)
            {
                var existingInterfaceUser = _interfaceUserRepository.Get(iu => iu.UserId == existingUser.Id.ToString());
                if (existingInterfaceUser == null)
                    await _interfaceUserRepository.CreateAsync(new InterfaceUser
                    {
                        CreatedByUserId = existingUser.Id.ToString(),
                        CreatedUTC = DateTime.UtcNow,
                        UserId = existingUser.Id.ToString(),
                        ChannelId = channel.Id.ToString(),
                        ExternalId = user.ExternalId,
                    });

                await _slackService.SendMessageAsync(channel.Key, channelId, $"Hello {user.FirstName}, it seems you already have a JOGL profile! You can log in <{url}|here>");
                _logger.LogDebug("Sent returning user message to {0}", user.ExternalId);
            }
            else
            {
                var userId = await _userService.CreateAsync(new Data.User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Status = UserStatus.Verified,
                    CreatedUTC = DateTime.UtcNow,
                });

                await _interfaceUserRepository.CreateAsync(new InterfaceUser
                {
                    CreatedByUserId = userId,
                    CreatedUTC = DateTime.UtcNow,
                    UserId = userId,
                    ChannelId = channel.Id.ToString(),
                    ExternalId = user.ExternalId,
                });

                await _membershipService.AddMembersAsync([new Membership {
                        CreatedByUserId = userId,
                        CreatedUTC = DateTime.UtcNow,
                        CommunityEntityId = channel.NodeId,
                        CommunityEntityType = CommunityEntityType.Node,
                        AccessLevel = AccessLevel.Member,
                        UserId = userId,
                    }]);

                await _slackService.SendMessageAsync(channel.Key, channelId, $"Hello {user.FirstName}, welcome to JOGL! We've set up your JOGL account on {user.Email}. You can set up your profile <{url}|here>");
                _logger.LogDebug("Sent new user message to {0}", user.ExternalId);
            }
        }
    }
}
