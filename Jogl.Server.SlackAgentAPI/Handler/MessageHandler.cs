using Jogl.Server.AI;
using Jogl.Server.Conversation.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using SlackNet;
using SlackNet.Events;

namespace Jogl.Server.SlackAgentAPI.Handler;

public class MessageHandler : IEventHandler<MessageEvent>
{
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ISystemValueRepository _systemValueRepository;
    private readonly IAIService _aiService;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(IServiceBusProxy serviceBusProxy, ISystemValueRepository systemValueRepository, IAIService aIService, ILogger<MessageHandler> logger)
    {
        _serviceBusProxy = serviceBusProxy;
        _systemValueRepository = systemValueRepository;
        _aiService = aIService;
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

    private async Task<string> GetTypeAsync(string message)
    {
        var prompt = _systemValueRepository.Get(sv => sv.Key == "ROUTER_PROMPT");
        if (prompt == null)
        {
            _logger.LogError("ROUTER_PROMPT system value missing");
            return string.Empty;
        }

        var allowedValues = new List<string>(["new_request", "consult_profile", "documentation", "feedback"]);
        var type = await _aiService.GetResponseAsync(string.Format(prompt.Value, string.Join(Environment.NewLine, allowedValues)), [new InputItem { FromUser = true, Text = message }], allowedValues, 0);
        return type;
    }

    public async Task HandleMessageAsync(MessageEvent slackEvent)
    {
        var type = await GetTypeAsync(slackEvent.Text);
        await _serviceBusProxy.SendAsync(new Message
        {
            ConversationSystem = Const.TYPE_SLACK,
            WorkspaceId = slackEvent.Team,
            ChannelId = slackEvent.Channel,
            ConversationId = slackEvent.Ts,
            Text = slackEvent.Text,
            UserId = slackEvent.User,
            Type = type,
            MessageId = slackEvent.Ts,
        }, "interface-message-received");
    }

    public async Task HandleReplyAsync(MessageEvent slackEvent)
    {
        await _serviceBusProxy.SendAsync(new Message
        {
            ConversationSystem = Const.TYPE_SLACK,
            WorkspaceId = slackEvent.Team,
            ChannelId = slackEvent.Channel,
            ConversationId = slackEvent.ThreadTs,
            Text = slackEvent.Text,
            UserId = slackEvent.User,
            Type = "deepdive",
            MessageId = slackEvent.Ts,
        }, "interface-message-received");
    }
}