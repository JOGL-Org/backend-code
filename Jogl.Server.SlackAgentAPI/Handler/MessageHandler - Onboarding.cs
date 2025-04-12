//using Jogl.Server.AI;
//using SlackNet;
//using SlackNet.Events;

//namespace Jogl.Server.SlackAgentAPI.Handler;

//public class MessageHandler : IEventHandler<MessageEvent>
//{
//    private readonly ISlackApiClient _slackApiClient;
//    private readonly IAIService _aiService;
//    private readonly ILogger<MessageHandler> _logger;
//    public MessageHandler(ISlackApiClient slackApiClient, IAIService aIService, ILogger<MessageHandler> logger)
//    {
//        _slackApiClient = slackApiClient;
//        _aiService = aIService;
//        _logger = logger;
//    }

//    public async Task Handle(MessageEvent slackEvent)
//    {
//        if (!string.IsNullOrEmpty(slackEvent.BotId) || !string.IsNullOrEmpty(slackEvent.AppId) || slackEvent.Type != "message" || !string.IsNullOrEmpty(slackEvent.Subtype))
//            return;

//        var history = await _slackApiClient.Conversations.Replies(slackEvent.Channel, slackEvent.ThreadTs, limit: 10);
//        var x = slackEvent.BotProfile;

//        var res = await _aiService.GetResponseAsync("You are a conversation agent. Your job is to ask the user for onboarding details: first name, last name, email. Ask for one thing at a time. At the end, return a json with the data you captured.",
//          history.Messages.Select(m => new InputItem { FromUser = string.IsNullOrEmpty(m.BotId), Text = m.Text }));

//        await _slackApiClient.Chat.PostMessage(new SlackNet.WebApi.Message
//        {
//            Channel = slackEvent.Channel,
//            Text = res,
//            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
//        });
//    }
//}
