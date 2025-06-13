using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.Data;
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
    private readonly IInterfaceMessageRepository _interfaceMessageRepository;
    private readonly IInterfaceUserRepository _interfaceUserRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(ISlackService slackService, IAgent agent, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceMessageRepository interfaceMessageRepository, IInterfaceUserRepository interfaceUserRepository, IUserRepository userRepository, ILogger<MessageHandler> logger)
    {
        _slackService = slackService;
        _agent = agent;
        _interfaceChannelRepository = interfaceChannelRepository;
        _interfaceMessageRepository = interfaceMessageRepository;
        _interfaceUserRepository = interfaceUserRepository;
        _userRepository = userRepository;
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

        if (string.IsNullOrEmpty(slackEvent.ThreadTs))
            await HandleMessageAsync(slackEvent, channel);
        else
            await HandleReplyAsync(slackEvent, channel);
    }

    public async Task HandleMessageAsync(MessageEvent slackEvent, InterfaceChannel channel)
    {
        //var interfaceUser = _interfaceUserRepository.Get(iu => iu.ExternalId == slackEvent.User);

        //var prev = await _slackService.GetPreviousMessage(channel.Key, slackEvent.Channel, slackEvent.Ts);
        //var interfaceMessage = prev != null ? _interfaceMessageRepository.Get(im => im.MessageId == prev.Id) : null;
        //if (interfaceMessage?.Tag == InterfaceMessage.TAG_ONBOARDING)
        //{
        //    await _userRepository.SetCurrentAsync(interfaceUser.UserId, slackEvent.Text);

        //    var messageText = string.Format("Thank you {0}! Your JOGL profile is now live.\r\nNow that I know more about what you do and what you’re into, I can help you find the right people and opportunities—right here in this space and across the entire JOGL network.\r\nYou can ask me anything, provide as much details as possible for better matches. While your JOGL profile is public, all your queries here remain confidential.\r\n", user.Profile.FirstName);
        //    await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, messageText);

        //    return;
        //}

        //var user = await _slackService.GetUserInfoAsync(channel.Key, slackEvent.User);

        //log incoming message
        var rootInterfaceMessage = new InterfaceMessage
        {
            CreatedUTC = DateTime.UtcNow,
            MessageId = slackEvent.Ts,
            ChannelId = channel.ExternalId,
            ConversationId = slackEvent.ThreadTs ?? slackEvent.Ts,
            UserId = slackEvent.User,
            Text = slackEvent.Text,
        };

        await _interfaceMessageRepository.CreateAsync(rootInterfaceMessage);

        var tempMessageId = await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, $"Your query is being processed now, your results should be available in a few seconds", slackEvent.Ts);

        var allUsers = await _slackService.ListWorkspaceUsersAsync(channel.Key);
        var emailHandles = allUsers.ToDictionary(u => u.Profile.Email, u => u.Id);

        var response = await _agent.GetInitialResponseAsync([new InputItem { FromUser = true, Text = slackEvent.Text }], emailHandles, channel.NodeId);
        var messageId = await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, response.Text, slackEvent.Ts);
        await _slackService.DeleteMessageAsync(channel.Key, slackEvent.Channel, tempMessageId);

        //log outgoing message
        await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
        {
            CreatedUTC = DateTime.UtcNow,
            MessageId = messageId,
            ChannelId = channel.ExternalId,
            ConversationId = slackEvent.ThreadTs ?? slackEvent.Ts,
            Text = response.Text,
            Tag = InterfaceMessage.TAG_SEARCH_USER,
        });

        //store context in root message
        rootInterfaceMessage.Context = response.Context;
        await _interfaceMessageRepository.UpdateAsync(rootInterfaceMessage);
    }

    public async Task HandleReplyAsync(MessageEvent slackEvent, InterfaceChannel channel)
    {
        //var interfaceUser = _interfaceUserRepository.Get(iu => iu.ExternalId == slackEvent.User);

        var rootInterfaceMessage = _interfaceMessageRepository.Get(m => m.ChannelId == channel.ExternalId && m.MessageId == slackEvent.ThreadTs);
        if (rootInterfaceMessage?.Context == null)
            return;

        //log incoming message
        await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
        {
            CreatedUTC = DateTime.UtcNow,
            MessageId = slackEvent.Ts,
            ChannelId = channel.ExternalId,
            ConversationId = slackEvent.ThreadTs ?? slackEvent.Ts,
            UserId = slackEvent.User,
            Text = slackEvent.Text,
        });

        var tempMessageId = await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, $"We will have a reply for you in a few moments", slackEvent.Ts);

        var messages = await _slackService.GetConversationAsync(channel.Key, slackEvent.Channel, slackEvent.ThreadTs ?? slackEvent.Ts);
        var followup = await _agent.GetFollowupResponseAsync(messages.Where(m => m.Id != tempMessageId).Select(m => new InputItem { FromUser = m.FromUser, Text = m.Text }), rootInterfaceMessage.Context);
        var replyMessageId = await _slackService.SendMessageAsync(channel.Key, slackEvent.Channel, followup.Text, slackEvent.Ts);
        await _slackService.DeleteMessageAsync(channel.Key, slackEvent.Channel, tempMessageId);

        //log outgoing message
        await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
        {
            CreatedUTC = DateTime.UtcNow,
            MessageId = replyMessageId,
            ChannelId = channel.ExternalId,
            ConversationId = slackEvent.ThreadTs ?? slackEvent.Ts,
            Text = followup.Text
        });
    }
}