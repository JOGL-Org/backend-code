using Jogl.Server.AI;
using SlackNet;
using SlackNet.Events;

namespace Jogl.Server.SlackAgentAPI.Handler;

public class ChannelHandler : IEventHandler<MemberJoinedChannel>
{
    private readonly ISlackApiClient _slackApiClient;
    private readonly ILogger<MessageHandler> _logger;
    public ChannelHandler(ISlackApiClient slackApiClient, IAIService aIService, ILogger<MessageHandler> logger)
    {
        _slackApiClient = slackApiClient;
        _logger = logger;
    }

    public async Task Handle(MemberJoinedChannel slackEvent)
    {
        var members = await _slackApiClient.Conversations.Members(slackEvent.Channel);
        foreach (var member in members.Members)
        {
            var userChannelId = await _slackApiClient.Conversations.Open([member]);
            await _slackApiClient.Chat.PostMessage(new SlackNet.WebApi.Message { Channel = userChannelId, Text = "Hello" });
        }
        
    }
}
