using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
using Jogl.Server.AI.Agent.DTO;
using Jogl.Server.Conversation.Data;
using Jogl.Server.ConversationCoordinator.Services;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Text;
using Jogl.Server.Verification;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.ConversationCoordinator
{
    public class MessageReceivedFunction : BaseFunction
    {
        private readonly IAgent _aiAgent;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceUserRepository _interfaceUserRepository;
        private readonly IInterfaceMessageRepository _interfaceMessageRepository;
        private readonly IUserRepository _userRepository;
        private readonly INodeRepository _nodeRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserVerificationService _userVerificationService;
        private readonly ITextService _textService;
        private readonly ILogger<MessageReceivedFunction> _logger;

        public MessageReceivedFunction(IAgent aiAgent, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceUserRepository interfaceUserRepository, IInterfaceMessageRepository interfaceMessageRepository, IOutputServiceFactory outputServiceFactory, IConfiguration configuration, IUserRepository userRepository, INodeRepository nodeRepository, IMembershipRepository membershipRepository, IUserVerificationService userVerificationService, ITextService textService, ILogger<MessageReceivedFunction> logger) : base(outputServiceFactory, configuration)
        {
            _aiAgent = aiAgent;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceMessageRepository = interfaceMessageRepository;
            _interfaceUserRepository = interfaceUserRepository;
            _userRepository = userRepository;
            _nodeRepository = nodeRepository;
            _membershipRepository = membershipRepository;
            _userVerificationService = userVerificationService;
            _textService = textService;
            _logger = logger;
        }

        [Function(nameof(MessageReceivedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.INTERFACE_MESSAGE_RECEIVED, Connection = "ConnectionString", AutoCompleteMessages = true)]
            ServiceBusReceivedMessage messageData,
            ServiceBusMessageActions messageActions)
        {
            var message = JsonSerializer.Deserialize<Message>(messageData.Body.ToString());
            if (message == null)
            {
                _logger.LogWarning("Message could not be deserialized: {messageBody}", messageData.Body.ToString());
                return;
            }

            var interfaceUser = _interfaceUserRepository.Get(iu => iu.ExternalId == message.UserId);
            switch (interfaceUser?.OnboardingStatus)
            {
                case null:
                    await ProcessIntroductionAsync(message);
                    break;
                case InterfaceUserOnboardingStatus.EmailPending:
                    await ProcessOnboardingEmailAsync(message, interfaceUser);
                    break;
                case InterfaceUserOnboardingStatus.CodePending:
                    await ProcessOnboardingCodeAsync(message, interfaceUser);
                    break;
                case InterfaceUserOnboardingStatus.CurrentWorkPending:
                    {
                        var user = _userRepository.Get(interfaceUser.UserId);
                        await ProcessOnboardingWorkAsync(message, interfaceUser, user);
                        break;
                    }
                default:
                    {
                        var user = _userRepository.Get(interfaceUser.UserId);
                        if (message.Type == "deepdive")
                            await ProcessReplyAsync(message, interfaceUser, user);
                        else
                            await ProcessMessageAsync(message, interfaceUser, user);

                        break;
                    }
            }
        }

        public async Task ProcessIntroductionAsync(Message conversation)
        {
            var outputService = _outputServiceFactory.GetService(conversation.ConversationSystem);
            var messageResultData = await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, ["Welcome to JOGL! Let's start by sharing your email address. If you are member of an organization that is using JOGL internally, make sure to use the email affiliated to that organization"]);

            await _interfaceUserRepository.CreateAsync(new InterfaceUser
            {
                ExternalId = conversation.UserId,
                ChannelId = conversation.ChannelId,
                CreatedUTC = DateTime.UtcNow,
            });
        }

        public async Task ProcessOnboardingEmailAsync(Message message, InterfaceUser interfaceUser)
        {
            var outputService = _outputServiceFactory.GetService(message.ConversationSystem);

            var email = message.Text;
            var user = _userRepository.Get(u => u.Email == email);
            if (user == null)
            {
                await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, ["We don't recognize that email. Please try a different email address."]);
                return;
            }

            await _userVerificationService.CreateAsync(user, VerificationAction.Verify, "", true);
            var messageResultData = await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, [$"We need to verify your email. A code has just been sent to {user.Email}, please share it back with me in this chat."]);

            interfaceUser.OnboardingStatus = InterfaceUserOnboardingStatus.CodePending;
            interfaceUser.UserId = user.Id.ToString();
            await _interfaceUserRepository.UpdateAsync(interfaceUser);

            //log outgoing messages
            //await LogMessagesAsync(messageResultData, message, InterfaceMessage.TAG_ONBOARDING_EMAIL_RECEIVED);
        }

        public async Task ProcessOnboardingCodeAsync(Message message, InterfaceUser interfaceUser)
        {
            var outputService = _outputServiceFactory.GetService(message.ConversationSystem);

            var code = message.Text;
            var user = _userRepository.Get(interfaceUser.UserId);
            var verificationResult = await _userVerificationService.VerifyAsync(user.Email, VerificationAction.Verify, code);
            switch (verificationResult.Status)
            {
                case VerificationStatus.Invalid:
                    await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, ["It seems that the code is invalid. Please make sure you are using the verification code from JOGL."]);
                    return;
                case VerificationStatus.Expired:
                    await _userVerificationService.CreateAsync(user, VerificationAction.Verify, "", true);
                    await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, ["It seems that the code has expired. We just sent you a new code to verify yourself with."]);
                    return;
            }

            var nodeName = GetNodeText(user.Id.ToString());
            var nodeId = GetNodeId(user.Id.ToString());
            await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, [$"Your email is confirmed, and you are a member of {nodeName}", $"Here is a quick bio based on the data we found about your experience: {_textService.StripHtml(user.Bio)}"]);
            var response = await _aiAgent.GetOnboardingResponseAsync([new InputItem { FromUser = true, Text = user.Bio }]);
            var messageResultData = await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, response.Text);

            //update interface user
            interfaceUser.OnboardingStatus = InterfaceUserOnboardingStatus.CurrentWorkPending;
            await _interfaceUserRepository.UpdateAsync(interfaceUser);

            //create interface channel
            await _interfaceChannelRepository.CreateAsync(new InterfaceChannel
            {
                ExternalId = message.ChannelId,
                NodeId = nodeId,
                Type = ChannelType.WhatsApp,
                CreatedUTC = DateTime.UtcNow,
            });

            //log outgoing messages
            await LogMessagesAsync(messageResultData, message, InterfaceMessage.TAG_ONBOARDING_CODE_RECEIVED);
        }

        public async Task ProcessOnboardingWorkAsync(Message message, InterfaceUser interfaceUser, Data.User user)
        {
            var outputService = _outputServiceFactory.GetService(message.ConversationSystem);
            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == message.WorkspaceId);

            var code = message.Text;

            var lastMessageId = _interfaceMessageRepository.Get(m => m.ChannelId == message.ChannelId && m.Tag == InterfaceMessage.TAG_ONBOARDING_CODE_RECEIVED).MessageId;
            var messages = await outputService.LoadConversationAsync(message.WorkspaceId, message.ChannelId, lastMessageId);
            if (messages.LastOrDefault()?.FromUser != true) //sometimes, whatsapp API doesn't load the latest message
                messages.Add(new InputItem { FromUser = true, Text = message.Text }); //and we need to inject it manually
            var response = await _aiAgent.GetOnboardingResponseAsync([new InputItem { FromUser = true, Text = user.Bio }, .. messages]);
            var messageResultData = await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, response.Text);

            if (!response.Stop)
                return;

            //log outgoing messages
            //await LogMessagesAsync(messageResultData, message, InterfaceMessage.TAG_ONBOARDING_COMPLETED);

            user.Current = response.Output;
            await _userRepository.UpdateAsync(user);

            var firstSearchResponse = await _aiAgent.GetFirstSearchResponseAsync(user.Current);
            var firstSearchResponseText = firstSearchResponse.Text.Single();
            var firstSearchResultData = await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, [$"Let me show you how to run a search. Feel free to rerun it later with more information. Based on what you told me, you are searching for: {firstSearchResponseText}"]);

            interfaceUser.OnboardingStatus = InterfaceUserOnboardingStatus.Onboarded;
            await _interfaceUserRepository.UpdateAsync(interfaceUser);

            await ProcessMessageAsync(new Message
            {
                ChannelId = message.ChannelId,
                ConversationId = message.ConversationId,
                MessageId = firstSearchResultData.Last().MessageId,
                ConversationSystem = message.ConversationSystem,
                Text = firstSearchResponseText,
                UserId = message.UserId,
                WorkspaceId = message.WorkspaceId
            }, interfaceUser, user);
        }

        public async Task ProcessMessageAsync(Message message, InterfaceUser interfaceUser, Data.User user)
        {
            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == message.ChannelId);

            //log incoming message
            var rootInterfaceMessage = new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = message.ConversationId,
                ChannelId = message.ChannelId,
                ConversationId = message.ConversationId,
                UserId = message.UserId,
                Text = message.Text,
            };

            await _interfaceMessageRepository.CreateAsync(rootInterfaceMessage);

            var mirrorConversationId = await MirrorConversationAsync(message.Text, user);

            var outputService = _outputServiceFactory.GetService(message.ConversationSystem);
            var indicatorId = await outputService.StartIndicatorAsync(message.WorkspaceId, message.ChannelId, message.ConversationId);

            var response = await GetMessageResponseAsync(message, channel, interfaceUser);
            var messageResultData = await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, response.Response.Text);
            await outputService.StopIndicatorAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, indicatorId);

            //log outgoing messages
            // await LogMessagesAsync(messageResultData, message);
            await MirrorRepliesAsync(mirrorConversationId, response.Response.Text);

            //store context in root message
            rootInterfaceMessage.MessageMirrorId = mirrorConversationId;
            rootInterfaceMessage.Tag = response.Tag;
            rootInterfaceMessage.Context = response.Response.Context;
            rootInterfaceMessage.OriginalQuery = response.Response.OriginalQuery;

            await _interfaceMessageRepository.UpdateAsync(rootInterfaceMessage);
        }

        public async Task ProcessReplyAsync(Message message, InterfaceUser interfaceUser, Data.User user)
        {
            var rootInterfaceMessage = _interfaceMessageRepository.Get(m => m.ChannelId == message.ChannelId && m.MessageId == message.ConversationId && m.Tag != null);
            if (rootInterfaceMessage?.Tag == null)
                return;

            await MirrorRepliesAsync(rootInterfaceMessage.MessageMirrorId, [message.Text], user);

            //log incoming message
            await _interfaceMessageRepository.CreateAsync(new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = message.MessageId,
                ChannelId = message.WorkspaceId,
                ConversationId = message.ConversationId,
                UserId = message.UserId,
                Text = message.Text
            });

            var outputService = _outputServiceFactory.GetService(message.ConversationSystem);
            var indicatorId = await outputService.StartIndicatorAsync(message.WorkspaceId, message.ChannelId, message.ConversationId);

            var response = await GetMessageDeepDiveResponseAsync(message, rootInterfaceMessage, interfaceUser);
            var messageResultData = await outputService.SendMessagesAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, response.Text);
            await MirrorRepliesAsync(rootInterfaceMessage.MessageMirrorId, response.Text);

            ////log outgoing message
            //await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            //{
            //    CreatedUTC = DateTime.UtcNow,
            //    MessageId = r.MessageId,
            //    ChannelId = message.ChannelId,
            //    ConversationId = message.ConversationId,
            //    Text = r.MessageText,
            //    Tag = rootInterfaceMessage.Tag
            //}).ToList());

            await outputService.StopIndicatorAsync(message.WorkspaceId, message.ChannelId, message.ConversationId, indicatorId);
        }

        private async Task<(string Tag, AgentResponse Response)> GetMessageResponseAsync(Message message, InterfaceChannel interfaceChannel, InterfaceUser interfaceUser)
        {
            switch (message.Type)
            {
                case "consult_profile":
                    var user = _userRepository.Get(interfaceUser.UserId);
                    var messages = await GetPreviousMessagesAsync(message);
                    var profileResponse = await _aiAgent.GetProfileResponseAsync(messages, user);
                    return (InterfaceMessage.TAG_CONSULT_PROFILE, profileResponse);
                case "new_request":
                default:
                    var searchResponse = await _aiAgent.GetInitialResponseAsync(message.Text, interfaceChannel?.NodeId, message.ConversationSystem);
                    return (InterfaceMessage.TAG_SEARCH_USER, searchResponse);
            }
        }

        private async Task<AgentResponse> GetMessageDeepDiveResponseAsync(Message message, InterfaceMessage rootInterfaceMessage, InterfaceUser interfaceUser)
        {
            switch (message.Type)
            {
                case "consult_profile":
                    var user = _userRepository.Get(interfaceUser.UserId);
                    var messages = await GetPreviousMessagesAsync(message);
                    var profileResponse = await _aiAgent.GetProfileResponseAsync(messages, user);
                    return profileResponse;
                case "new_request":
                default:
                    return await _aiAgent.GetFollowupResponseAsync([new InputItem { FromUser = true, Text = message.Text }], rootInterfaceMessage.Context, rootInterfaceMessage.OriginalQuery, message.ConversationSystem);
            }
        }

        private async Task<List<InputItem>> GetPreviousMessagesAsync(Message message)
        {
            var outputService = _outputServiceFactory.GetService(message.ConversationSystem);
            var messages = await outputService.LoadConversationAsync(message.WorkspaceId, message.ChannelId, message.ConversationId);
            if (messages.LastOrDefault()?.FromUser != true) //sometimes, the whatsapp API doesn't load the latest message
                messages.Add(new InputItem { FromUser = true, Text = message.Text }); //and we need to inject it manually

            return messages;
        }

        private async Task LogMessagesAsync(IEnumerable<DTO.MessageResult> messageResultData, Message message, string tag = default)
        {
            await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = r.MessageId,
                ChannelId = message.WorkspaceId,
                ConversationId = message.ConversationId,
                Text = r.MessageText,
                Tag = tag,
            }).ToList());
        }

        private string GetNodeText(string userId)
        {
            var nodeIds = _membershipRepository.Query(m => m.UserId == userId && m.CommunityEntityType == CommunityEntityType.Node)
                .ToList()
                .Select(m => m.CommunityEntityId)
                .ToList();

            var nodes = _nodeRepository.Get(nodeIds);
            switch (nodes.Count)
            {
                case 1:
                    return nodes.Single().Title;
                default:
                    return $"{nodes.Count} communities";
            }
        }

        private string? GetNodeId(string userId)
        {
            var nodeIds = _membershipRepository.Query(m => m.UserId == userId && m.CommunityEntityType == CommunityEntityType.Node)
                .ToList()
                .Select(m => m.CommunityEntityId)
                .ToList();

            var nodes = _nodeRepository.Get(nodeIds);
            if (!nodes.Any())
                return null;

            return nodes.First().Id.ToString();
        }

    }
}
