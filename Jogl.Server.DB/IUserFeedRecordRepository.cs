using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserFeedRecordRepository : IRepository<UserFeedRecord>
    {
        Task SetFeedListedAsync(string userId, string feedId, DateTime listedUTC);
        Task SetFeedOpenedAsync(string userId, string feedId, DateTime openedUTC);
        Task SetFeedReadAsync(string userId, string feedId, DateTime readUTC);
        Task SetFeedWrittenAsync(string userId, string feedId, DateTime writeUTC);
        Task SetFeedMentionAsync(string userId, string feedId, DateTime mentionUTC);
        Task DeleteAsync(string userId, string feedId);
    }
}