using Jogl.Server.Conversation.Data;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;

namespace Jogl.Server.Business
{
    public class AgentService(IChannelRepository channelRepository, IContentEntityRepository contentEntityRepository, IInterfaceUserRepository interfaceUserRepository, IServiceBusProxy serviceBusProxy) : IAgentService
    {
        public async Task NotifyAsync(ContentEntity contentEntity)
        {
            if (string.IsNullOrEmpty(contentEntity.CreatedByUserId))
                return;

            if (contentEntity.Type != ContentEntityType.Message)
                return;

            var channel = channelRepository.Get(contentEntity.FeedId);
            if (channel?.Key != "USER_SEARCH")
                return;

            //ensure interface user is created
            var interfaceUser = interfaceUserRepository.Get(u => u.UserId == contentEntity.CreatedByUserId && u.Type == Const.TYPE_JOGL);
            if (interfaceUser == null)
            {
                await interfaceUserRepository.CreateAsync(new InterfaceUser
                {
                    UserId = contentEntity.CreatedByUserId,
                    ExternalId = contentEntity.CreatedByUserId,
                    Type = Const.TYPE_JOGL,
                    OnboardingStatus = InterfaceUserOnboardingStatus.Onboarded
                });
            }

            var rootMessage = contentEntityRepository
                .Query(ce => ce.FeedId == contentEntity.FeedId && ce.Id != contentEntity.Id)
                .Sort(Data.Util.SortKey.CreatedDate)
                .ToList()
                .FirstOrDefault();

            await serviceBusProxy.SendAsync(new Message
            {
                ConversationSystem = Const.TYPE_JOGL,
                WorkspaceId = contentEntity.FeedId,
                ChannelId = contentEntity.FeedId,
                ConversationId = rootMessage == null ? contentEntity.Id.ToString() : rootMessage.Id.ToString(),
                MessageId = contentEntity.Id.ToString(),
                Text = contentEntity.Text,
                UserId = contentEntity.CreatedByUserId,
                Type = rootMessage == null ? "new_request" : "deepdive",
            }, "interface-message-received");

        }
    }
}