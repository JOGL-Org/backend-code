//using Jogl.Server.AI;
//using SlackNet;
//using SlackNet.Events;

//namespace Jogl.Server.SlackAgentAPI.Handler;

//public class BotAddedHandler : IEventHandler<BotAdded>
//{
//    private readonly ISlackApiClient _slackApiClient;
//    private readonly ILogger<MessageHandler> _logger;
//    public BotAddedHandler(ISlackApiClient slackApiClient, IAIService aIService, ILogger<MessageHandler> logger)
//    {
//        _slackApiClient = slackApiClient;
//        _logger = logger;
//    }

//    public async Task Handle(BotAdded slackEvent)
//    {
//        //var members = await _slackApiClient.Conversations.Members("C08LJPULL3H");
//        //var userChannelId = await _slackApiClient.Conversations.Open([members.Members[0]]);
//        //await _slackApiClient.Chat.PostMessage(new SlackNet.WebApi.Message { Channel = userChannelId, Text = "Hello" });
//    }
//}
