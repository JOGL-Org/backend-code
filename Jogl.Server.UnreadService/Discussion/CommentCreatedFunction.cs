using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier.Discussion
{
    public class CommentCreatedFunction
    {
        private readonly IUserContentEntityRecordRepository _userContentEntityRecordRepository;
        private readonly IUserFeedRecordRepository _userFeedRecordRepository;

        public CommentCreatedFunction(IUserContentEntityRecordRepository userContentEntityRecordRepository, IUserFeedRecordRepository userFeedRecordRepository, ILogger<CommentCreatedFunction> logger)
        {
            _userContentEntityRecordRepository = userContentEntityRecordRepository;
            _userFeedRecordRepository = userFeedRecordRepository;
        }

        [Function(nameof(CommentCreatedFunction))]
        public async Task RunCommentsAsync(
            [ServiceBusTrigger("comment-created", "unread", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var comment = JsonSerializer.Deserialize<Comment>(message.Body.ToString());

            //find users that follow the thread
            var userIds = _userContentEntityRecordRepository.Query(ucer => ucer.ContentEntityId == comment.ContentEntityId)
                .Filter(ucer => ucer.FollowedUTC.HasValue)
                .Filter(ucer => ucer.UserId != comment.CreatedByUserId)
                .ToList()
                .Select(ucer => ucer.UserId)
                .ToList();

            var userFeedRecords = _userFeedRecordRepository.Query(ufr => userIds.Contains(ufr.UserId))
                .Filter(ufr => ufr.FeedId == comment.FeedId)
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
