using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.DB;
using SlackNet;
using SlackNet.Events;

namespace Jogl.Server.SlackAgentAPI.Handler;

public class MessageHandler : IEventHandler<MessageEvent>
{
    private readonly ISlackApiClient _slackApiClient;
    private readonly IAgent _agent;
    private readonly IInterfaceChannelRepository _interfaceChannelRepository;
    private readonly ILogger<MessageHandler> _logger;
    public MessageHandler(ISlackApiClient slackApiClient, IAgent agent, IInterfaceChannelRepository interfaceChannelRepository, ILogger<MessageHandler> logger)
    {
        _slackApiClient = slackApiClient;
        _agent = agent;
        _interfaceChannelRepository = interfaceChannelRepository;
        _logger = logger;
    }

    private ISlackApiClient GetClient(string key)
    {
        return _slackApiClient.WithAccessToken(key);
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

        if (string.IsNullOrEmpty(channel.Key))
        {
            _logger.LogWarning("Slack workspace missing a key: {team}", slackEvent.Team);
            return;
        }

        if (string.IsNullOrEmpty(channel?.NodeId))
        {
            _logger.LogWarning("Slack workspace missing a node id: {team}", slackEvent.Team);
            return;
        }

        var client = GetClient(channel.Key);
        var tempMessageTs = await SendMessageAsync(client, slackEvent.Channel, $"Your query is being processed now, your results should be available in a few seconds", slackEvent.Ts);
        var messages = await GetHistoryAsync(client, slackEvent.Channel, slackEvent.ThreadTs ?? slackEvent.Ts, [tempMessageTs]);

        var response = await _agent.GetResponseAsync(messages, channel.NodeId);
        await SendMessageAsync(client, slackEvent.Channel, response, slackEvent.Ts);
        await DeleteMessageAsync(client, slackEvent.Channel, tempMessageTs);
    }

    private async Task<string> SendMessageAsync(ISlackApiClient client, string channel, string text, string ts)
    {
        var res = await client.Chat.PostMessage(new SlackNet.WebApi.Message
        {
            Channel = channel,
            Text = text,
            ThreadTs = ts
        });

        return res.Ts;
    }

    private async Task<List<InputItem>> GetHistoryAsync(ISlackApiClient client, string channel, string threadTs, IEnumerable<string> ignoreTs)
    {
        var history = await client.Conversations.Replies(channel, threadTs, limit: 10);
        return history.Messages.Where(m => !ignoreTs.Contains(m.Ts)).Select(m => new InputItem { FromUser = string.IsNullOrEmpty(m.BotId), Text = m.Text }).ToList();
    }

    private async Task DeleteMessageAsync(ISlackApiClient client, string channel, string ts)
    {
        await client.Chat.Delete(ts, channel, true);
    }
}