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

        public CommentCreatedFunction(IUserContentEntityRecordRepository userContentEntityRecordRepository, ILogger<CommentCreatedFunction> logger)
        {
            _userContentEntityRecordRepository = userContentEntityRecordRepository;
        }

        [Function(nameof(CommentCreatedFunction))]
        public async Task RunCommentsAsync(
            [ServiceBusTrigger("comment-created", "unread", Connection = "ConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var comment = JsonSerializer.Deserialize<Comment>(message.Body.ToString());

            //find users that follow content entity
            var userContentEntityRecords = _userContentEntityRecordRepository.Query(ucer => ucer.ContentEntityId == comment.ContentEntityId)
                .Filter(ucer => ucer.FollowedUTC.HasValue)
                .Filter(ucer => ucer.UserId != comment.CreatedByUserId)
                .ToList();

            //mark their content entity as unread
            foreach (var ucer in userContentEntityRecords)
            {
                ucer.Unread = true;
                await _userContentEntityRecordRepository.UpdateAsync(ucer);
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
