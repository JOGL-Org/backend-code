using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.AI;
using Jogl.Server.AI.Agent;
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
    public class ConversationCreatedFunction : BaseFunction
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
        private readonly ILogger<ConversationCreatedFunction> _logger;

        public ConversationCreatedFunction(IAgent aiAgent, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceUserRepository interfaceUserRepository, IInterfaceMessageRepository interfaceMessageRepository, IOutputServiceFactory outputServiceFactory, IConfiguration configuration, IUserRepository userRepository, INodeRepository nodeRepository, IMembershipRepository membershipRepository, IUserVerificationService userVerificationService, ITextService textService, ILogger<ConversationCreatedFunction> logger) : base(outputServiceFactory, configuration)
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

        [Function(nameof(ConversationCreatedFunction))]
        public async Task RunInvitesAsync(
            [ServiceBusTrigger(Const.CONVERSATION_CREATED, Connection = "ConnectionString", AutoCompleteMessages = true)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var conversation = JsonSerializer.Deserialize<ConversationCreated>(message.Body.ToString());

            var interfaceUser = _interfaceUserRepository.Get(iu => iu.ExternalId == conversation.UserId);
            switch (interfaceUser?.OnboardingStatus)
            {
                case null:
                    await ProcessIntroductionAsync(conversation);
                    break;
                case InterfaceUserOnboardingStatus.EmailPending:
                    await ProcessOnboardingEmailAsync(conversation, interfaceUser);
                    break;
                case InterfaceUserOnboardingStatus.CodePending:
                    await ProcessOnboardingCodeAsync(conversation, interfaceUser);
                    break;
                case InterfaceUserOnboardingStatus.CurrentWorkPending:
                    await ProcessOnboardingWorkAsync(conversation, interfaceUser);
                    break;
                default:
                    await ProcessConversationAsync(conversation);
                    break;
            }
        }

        public async Task ProcessIntroductionAsync(ConversationCreated conversation)
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

        public async Task ProcessOnboardingEmailAsync(ConversationCreated conversation, InterfaceUser interfaceUser)
        {
            var outputService = _outputServiceFactory.GetService(conversation.ConversationSystem);

            var email = conversation.Text;
            var user = _userRepository.Get(u => u.Email == email);
            if (user == null)
            {
                await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, ["We don't recognize that email. Please try a different email address."]);
                return;
            }

            await _userVerificationService.CreateAsync(user, VerificationAction.Verify, "", true);
            var messageResultData = await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, [$"We need to verify your email. A code has just been sent to {user.Email}, please share it back with me in this chat."]);

            interfaceUser.OnboardingStatus = InterfaceUserOnboardingStatus.CodePending;
            interfaceUser.UserId = user.Id.ToString();
            await _interfaceUserRepository.UpdateAsync(interfaceUser);

            //log outgoing messages
            await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = r.MessageId,
                ChannelId = conversation.WorkspaceId,
                ConversationId = conversation.ConversationId,
                Text = r.MessageText,
                Tag = InterfaceMessage.TAG_ONBOARDING_EMAIL_RECEIVED,
            }).ToList());
        }

        public async Task ProcessOnboardingCodeAsync(ConversationCreated conversation, InterfaceUser interfaceUser)
        {
            var outputService = _outputServiceFactory.GetService(conversation.ConversationSystem);

            var code = conversation.Text;
            var user = _userRepository.Get(interfaceUser.UserId);
            var verificationResult = await _userVerificationService.VerifyAsync(user.Email, VerificationAction.Verify, code);
            switch (verificationResult.Status)
            {
                case VerificationStatus.Invalid:
                    await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, ["It seems that the code is invalid. Please make sure you are using the verification code from JOGL."]);
                    return;
                case VerificationStatus.Expired:
                    await _userVerificationService.CreateAsync(user, VerificationAction.Verify, "", true);
                    await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, ["It seems that the code has expired. We just sent you a new code to verify yourself with."]);
                    return;
            }

            var nodeName = GetNodeText(user.Id.ToString());
            var messageResultData = await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, [$"Your email is confirmed, and you are a member of {nodeName}", $"Here is a quick bio based on the data we found about your experience: {_textService.StripHtml(user.Bio)}", $"Please tell us what you're currently working on."]);

            interfaceUser.OnboardingStatus = InterfaceUserOnboardingStatus.CurrentWorkPending;
            await _interfaceUserRepository.UpdateAsync(interfaceUser);

            //log outgoing messages
            await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = r.MessageId,
                ChannelId = conversation.WorkspaceId,
                ConversationId = conversation.ConversationId,
                Text = r.MessageText,
                Tag = InterfaceMessage.TAG_ONBOARDING_CODE_RECEIVED,
            }).ToList());
        }

        public async Task ProcessOnboardingWorkAsync(ConversationCreated conversation, InterfaceUser interfaceUser)
        {
            var outputService = _outputServiceFactory.GetService(conversation.ConversationSystem);
            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == conversation.WorkspaceId);

            var code = conversation.Text;
            var user = _userRepository.Get(interfaceUser.UserId);

            var lastMessageId = _interfaceMessageRepository.Get(m => m.ChannelId == conversation.ChannelId && m.Tag == InterfaceMessage.TAG_ONBOARDING_CODE_RECEIVED).MessageId;
            var messages = await outputService.LoadConversationAsync(conversation.WorkspaceId, conversation.ChannelId, lastMessageId);
            if (!messages.Last().FromUser) //sometimes, the whatsapp API doesn't load the latest message
                messages.Add(new InputItem { FromUser = true, Text = conversation.Text });
            var response = await _aiAgent.GetOnboardingResponseAsync([new InputItem { FromUser = true, Text = "Hello" }, .. messages], user.Bio);
            if (response.Stop)
            {
                user.Current = response.Output;
                await _userRepository.UpdateAsync(user);

                interfaceUser.OnboardingStatus = InterfaceUserOnboardingStatus.Onboarded;
                await _interfaceUserRepository.UpdateAsync(interfaceUser);
            }

            var messageResultData = await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, response.Text);

            //log outgoing messages
            await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = r.MessageId,
                ChannelId = conversation.WorkspaceId,
                ConversationId = conversation.ConversationId,
                Text = r.MessageText,
                Tag = InterfaceMessage.TAG_ONBOARDING_COMPLETED,
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

        public async Task ProcessConversationAsync(ConversationCreated conversation)
        {
            var channel = _interfaceChannelRepository.Get(ic => ic.ExternalId == conversation.WorkspaceId);

            //log incoming message
            var rootInterfaceMessage = new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = conversation.ConversationId,
                ChannelId = conversation.ChannelId,
                ConversationId = conversation.ConversationId,
                UserId = conversation.UserId,
                Text = conversation.Text,
            };

            await _interfaceMessageRepository.CreateAsync(rootInterfaceMessage);

            var mirrorConversationId = await MirrorConversationAsync(conversation.Text);

            var outputService = _outputServiceFactory.GetService(conversation.ConversationSystem);
            var indicatorId = await outputService.StartIndicatorAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId);

            var response = await _aiAgent.GetInitialResponseAsync(conversation.Text, channel?.NodeId, conversation.ConversationSystem);
            var messageResultData = await outputService.SendMessagesAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, response.Text);
            await outputService.StopIndicatorAsync(conversation.WorkspaceId, conversation.ChannelId, conversation.ConversationId, indicatorId);

            //log outgoing messages
            await _interfaceMessageRepository.CreateAsync(messageResultData.Select(r => new InterfaceMessage
            {
                CreatedUTC = DateTime.UtcNow,
                MessageId = r.MessageId,
                ChannelId = conversation.WorkspaceId,
                ConversationId = conversation.ConversationId,
                Text = r.MessageText,
                Tag = InterfaceMessage.TAG_SEARCH_USER,
            }).ToList());

            await MirrorRepliesAsync(mirrorConversationId, response.Text);

            //store context in root message
            rootInterfaceMessage.MessageMirrorId = mirrorConversationId;
            rootInterfaceMessage.Tag = InterfaceMessage.TAG_SEARCH_USER;
            rootInterfaceMessage.Context = response.Context;
            rootInterfaceMessage.OriginalQuery = response.OriginalQuery;

            await _interfaceMessageRepository.UpdateAsync(rootInterfaceMessage);
        }
    }
}
