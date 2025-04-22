using Jogl.Server.Conversation.Data;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using Jogl.Server.Slack;
using SlackNet;
using SlackNet.Events;

namespace Jogl.Server.SlackAgentAPI.Handler;

public class TeamJoinHandler : IEventHandler<TeamJoin>
{
    private readonly ISlackService _slackService;
    private readonly INotificationFacade _notificationFacade;
    private readonly IInterfaceChannelRepository _interfaceChannelRepository;
    private readonly ILogger<TeamJoinHandler> _logger;

    public TeamJoinHandler(ISlackService slackService, INotificationFacade notificationFacade, IInterfaceChannelRepository interfaceChannelRepository, ILogger<TeamJoinHandler> logger)
    {
        _slackService = slackService;
        _notificationFacade = notificationFacade;
        _interfaceChannelRepository = interfaceChannelRepository;
        _logger = logger;
    }

    public async Task Handle(TeamJoin slackEvent)
    {
        var teamId = slackEvent.User.TeamId;

        var channel = _interfaceChannelRepository.Get(c => c.ExternalId == teamId);
        if (channel == null)
        {
            await _interfaceChannelRepository.CreateAsync(new Data.InterfaceChannel { ExternalId = teamId });
        }

        if (string.IsNullOrEmpty(channel?.Key))
        {
            _logger.LogWarning("Slack workspace missing a key: {0}", teamId);
            return;
        }

        if (string.IsNullOrEmpty(channel?.NodeId))
        {
            _logger.LogWarning("Slack workspace missing a node id: {0}", teamId);
            return;
        }

        var user = await _slackService.GetUserInfoAsync(channel.Key, slackEvent.User.Id);
        await _notificationFacade.NotifyAsync(Const.USER_JOINED_INTERFACE_CHANNEL, new UserJoined
        {
            ChannelExternalId = user.TeamId,
            User = new Conversation.Data.User
            {
                Email = user.Profile.Email,
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                ExternalId = user.Id,
            }
        });
    }
}
