using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.DB;
using Jogl.Server.Slack;
using SlackNet;
using SlackNet.Events;

namespace Jogl.Server.SlackAgentAPI.Handler;

public class MessageHandler : IEventHandler<MessageEvent>
{
    private readonly ISlackService _slackService;
    private readonly IAgent _agent;
    private readonly IInterfaceChannelRepository _interfaceChannelRepository;
    private readonly ILogger<MessageHandler> _logger;
    public MessageHandler(ISlackService slackService, IAgent agent, IInterfaceChannelRepository interfaceChannelRepository, ILogger<MessageHandler> logger)
    {
        _slackService = slackService;
        _agent = agent;
        _interfaceChannelRepository = interfaceChannelRepository;
        _logger = logger;
    }

    public async Task Handle(MessageEvent slackEvent)
    {
        if (!string.IsNullOrEmpty(slackEvent.BotId) || !string.IsNullOrEmpty(slackEvent.AppId) || slackEvent.Type != "message" || !string.IsNullOrEmpty(slackEvent.Subtype))
            return;

        var channel = _interfaceChannelRepository.Get(c => c.ExternalId == slackEvent.Team);
        if (channel == null)
        {
            await _interfaceChannelRepository.CreateAsync(new Data.InterfaceChannel { ExternalId = slackEvent.Team });
        }

        if (string.IsNullOrEmpty(channel?.Key))
        {
            _logger.LogWarning("Slack workspace missing a key: {team}", slackEvent.Team);
            return;
        }

        if (string.IsNullOrEmpty(channel?.NodeId))
        {
            _logger.LogWarning("Slack workspace missing a node id: {team}", slackEvent.Team);
            return;
        }

        var messages = await _slackService.GetConversationAsync(channel.Key, slackEvent.Channel, slackEvent.ThreadTs ?? slackEvent.Ts);
        var tempMessageId = await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, $"Your query is being processed now, your results should be available in a few seconds", slackEvent.Ts);

        var response = await _agent.GetResponseAsync(messages.Select(m => new InputItem { FromUser = m.FromUser, Text = m.Text }), channel.NodeId);
        await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, response, slackEvent.Ts);
        await _slackService.DeleteMessageAsync(channel.Key, slackEvent.Channel, tempMessageId);
    }
}