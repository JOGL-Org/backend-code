//using Jogl.Server.Conversation.Data;
//using Jogl.Server.DB;
//using Jogl.Server.Notifications;
//using Jogl.Server.Slack;
//using SlackNet;
//using SlackNet.Events;

//namespace Jogl.Server.SlackAgentAPI.Handler;

//public class TeamJoinHandler : IEventHandler<TeamJoin>
//{
//    private readonly ISlackService _slackService;
//    private readonly INotificationFacade _notificationFacade;
//    private readonly IInterfaceChannelRepository _interfaceChannelRepository;
//    private readonly ILogger<TeamJoinHandler> _logger;

//    public TeamJoinHandler(ISlackService slackService, INotificationFacade notificationFacade, IInterfaceChannelRepository interfaceChannelRepository, ILogger<TeamJoinHandler> logger)
//    {
//        _slackService = slackService;
//        _notificationFacade = notificationFacade;
//        _interfaceChannelRepository = interfaceChannelRepository;
//        _logger = logger;
//    }

//    public async Task Handle(TeamJoin slackEvent)
//    {
//        var teamId = slackEvent.User.TeamId;

//        var channel = _interfaceChannelRepository.Get(c => c.ExternalId == teamId);
//        if (channel == null)
//        {
//            await _interfaceChannelRepository.CreateAsync(new Data.InterfaceChannel { ExternalId = teamId });
//        }

//        if (string.IsNullOrEmpty(channel?.Key))
//        {
//            _logger.LogWarning("Slack workspace missing a key: {0}", teamId);
//            return;
//        }

//        if (string.IsNullOrEmpty(channel?.NodeId))
//        {
//            _logger.LogWarning("Slack workspace missing a node id: {0}", teamId);
//            return;
//        }

//        var user = await _slackService.GetUserInfoAsync(channel.Key, slackEvent.User.Id);
//        //await _notificationFacade.NotifyAsync(Const.USER_JOINED_INTERFACE_CHANNEL, new UserJoined
//        //{
//        //    ChannelExternalId = user.TeamId,
//        //    User = new Conversation.Data.User
//        //    {
//        //        Email = user.Profile.Email,
//        //        FirstName = user.Profile.FirstName,
//        //        LastName = user.Profile.LastName,
//        //        ExternalId = user.Id,
//        //    }
//        //});

//        var channelId = await _slackService.GetUserChannelIdAsync(channel.Key, slackEvent.User.Id);
//        await _slackService.SendMessageAsync(channel.Key, channelId, ":wave: *Welcome to the JOGL Network agent* — your AI-powered matchmaker for your community and beyond!\r\n\r\nLooking for collaborators or experts? Just post a message like:\r\n>_“I’m looking for people who’ve worked on zero-knowledge proofs”_\r\n>or\r\n>_“Who could help with this project?”_ (include a short abstract)\r\n:brain: JOGL network will *reply directly to your post* with matching profiles from the community. *Interact with the agent’s answers* to explore deeper.\r\n:pushpin: Important: Start every new search as a new post (not a reply).");
//    }
//}
