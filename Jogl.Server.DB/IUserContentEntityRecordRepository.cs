using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserContentEntityRecordRepository : IRepository<UserContentEntityRecord>
    {
        Task SetContentEntityReadAsync(string userId, string feedId, string contentEntityId, DateTime readUTC);
        Task SetContentEntityWrittenAsync(string userId, string feedId, string contentEntityId, DateTime writeUTC);
        Task SetContentEntityMentionAsync(string userId, string feedId, string contentEntityId, DateTime mentionUTC);
    }
}