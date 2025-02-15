using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public class ContentEntityCreatedFunction
    {
        private readonly IUserFeedRecordRepository _userFeedRecordRepository;

        public ContentEntityCreatedFunction(IUserFeedRecordRepository userFeedRecordRepository, ILogger<CommentCreatedFunction> logger)
        {
            _userFeedRecordRepository = userFeedRecordRepository;
        }

        [Function(nameof(ContentEntityCreatedFunction))]
        public async Task RunCommentsAsync(
            [ServiceBusTrigger("content-entity-created", "unread", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var contentEntity = JsonSerializer.Deserialize<ContentEntity>(message.Body.ToString());

            //find users that follow the feed
            var userFeedRecords = _userFeedRecordRepository.Query(ufr => ufr.FeedId == contentEntity.FeedId)
                .Filter(ufr => ufr.FollowedUTC.HasValue)
                .Filter(ufr => !ufr.Unread)
                .ToList();

            //mark their feed as unread
            foreach (var userFeedRecord in userFeedRecords)
            {
                userFeedRecord.Unread = true;
                await _userFeedRecordRepository.UpdateAsync(userFeedRecord);
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
