using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Jogl.Server.WebSocketService.Sockets;

namespace Jogl.Server.WebSocketService
{
    public class Service : IHostedService, IDisposable
    {
        private readonly IServiceBusProxy _serviceBusProxy;
        private readonly IWebSocketGateway _socketGateway;
        private readonly IUserFeedRecordRepository _userFeedRecordRepository;
        private readonly ILogger<Service> _logger;

        public Service(IServiceBusProxy serviceBusProxy, IWebSocketGateway socketGateway, IUserFeedRecordRepository userFeedRecordRepository, ILogger<Service> logger)
        {
            _serviceBusProxy = serviceBusProxy;
            _socketGateway = socketGateway;
            _userFeedRecordRepository = userFeedRecordRepository;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Socket Service is starting.");

            await _serviceBusProxy.SubscribeAsync("comment-created", "sockets", async (Comment comment) =>
            {
                //notify via websockets
                var userFeeds = _userFeedRecordRepository.List(ufr => ufr.FeedId == comment.FeedId && !ufr.Muted && !ufr.Deleted);
                foreach (var userFeed in userFeeds)
                {
                    await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.FeedActivity, TopicId = userFeed.UserId, SubjectId = userFeed.FeedId });
                }

                await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.CommentInPost, TopicId = comment.ContentEntityId, SubjectId = comment.FeedId });

                foreach (var mention in comment.Mentions)
                {
                    await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.Mention, TopicId = mention.EntityId, SubjectId = mention.OriginFeedId });
                }
            });

            await _serviceBusProxy.SubscribeAsync("content-entity-created", "sockets", async (ContentEntity entity) =>
            {
                //notify via websockets
                var userFeeds = _userFeedRecordRepository.List(ufr => ufr.FeedId == entity.FeedId && !ufr.Muted && !ufr.Deleted);
                foreach (var userFeed in userFeeds)
                {
                    await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.FeedActivity, TopicId = userFeed.UserId, SubjectId = userFeed.FeedId });
                }
                await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.PostInFeed, TopicId = entity.FeedId, SubjectId = entity.FeedId });

                foreach (var mention in entity.Mentions)
                {
                    await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.Mention, TopicId = mention.EntityId, SubjectId = mention.OriginFeedId });
                }
            });

            await _serviceBusProxy.SubscribeAsync("notification-created", "sockets", async (Notification notification) =>
            {
                //notify via websockets
                await _socketGateway.SendMessageAsync(new SocketServerMessage { Type = ServerMessageType.Notification, TopicId = notification.UserId });
            });


            _logger.LogInformation("Socket Service successfully started.");
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Socket Service is stopping.");
        }

        public void Dispose()
        {
        }
    }
}
