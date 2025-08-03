using Jogl.Server.Conversation.Data;
using Jogl.Server.ServiceBus;
using SlackNet;
using SlackNet.Events;

namespace Jogl.Server.SlackAgentAPI.Handler;

public class MessageHandler : IEventHandler<MessageEvent>
{
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(IServiceBusProxy serviceBusProxy, ILogger<MessageHandler> logger)
    {
        _serviceBusProxy = serviceBusProxy;
        _logger = logger;
    }

    public async Task Handle(MessageEvent slackEvent)
    {
        if (!string.IsNullOrEmpty(slackEvent.BotId) || !string.IsNullOrEmpty(slackEvent.AppId) || slackEvent.Type != "message" || !string.IsNullOrEmpty(slackEvent.Subtype))
            return;

        _logger.LogInformation("Received slack message {messageId}", slackEvent.Ts);

        if (string.IsNullOrEmpty(slackEvent.ThreadTs))
            await HandleMessageAsync(slackEvent);
        else
            await HandleReplyAsync(slackEvent);
    }

    public async Task HandleMessageAsync(MessageEvent slackEvent)
    {
        await _serviceBusProxy.SendAsync(new ConversationCreated
        {
            ConversationSystem = Const.TYPE_SLACK,
            WorkspaceId = slackEvent.Team,
            ChannelId = slackEvent.Channel,
            ConversationId = slackEvent.Ts,
            Text = slackEvent.Text,
            UserId = slackEvent.User
        }, "conversation-created");
    }

    public async Task HandleReplyAsync(MessageEvent slackEvent)
    {
        await _serviceBusProxy.SendAsync(new ConversationReplyCreated
        {
            ConversationSystem = Const.TYPE_SLACK,
            WorkspaceId = slackEvent.Team,
            ChannelId = slackEvent.Channel,
            ConversationId = slackEvent.ThreadTs,
            MessageId = slackEvent.Ts,
            Text = slackEvent.Text,
            UserId = slackEvent.User
        }, "conversation-reply-created");
    }
}