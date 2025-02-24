using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class UserContentEntityRecordRepository : BaseRepository<UserContentEntityRecord>, IUserContentEntityRecordRepository
    {
        public UserContentEntityRecordRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "userContentEntityRecords";

        public async Task SetContentEntityReadAsync(string userId, string feedId, string contentEntityId, DateTime readUTC)
        {
            var filter = Builders<UserContentEntityRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserContentEntityRecord>.Filter.Eq(r => r.ContentEntityId, contentEntityId) & Builders<UserContentEntityRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserContentEntityRecord>.Update.Set(r => r.LastReadUTC, readUTC)
                                                                 .Set(r => r.UpdatedUTC, readUTC)
                                                                 .Set(r => r.UpdatedByUserId, userId)
                                                                 .Set(r => r.Unread, false)
                                                                 .SetOnInsert(r => r.FeedId, feedId)
                                                                 .SetOnInsert(r => r.ContentEntityId, contentEntityId)
                                                                 .SetOnInsert(r => r.UserId, userId)
                                                                 .SetOnInsert(r => r.Deleted, false);

            await UpsertAsync(filter, update);
        }

        public async Task SetContentEntityWrittenAsync(string userId, string feedId, string contentEntityId, DateTime writeUTC)
        {
            var filter = Builders<UserContentEntityRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserContentEntityRecord>.Filter.Eq(r => r.ContentEntityId, contentEntityId) & Builders<UserContentEntityRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserContentEntityRecord>.Update.Set(r => r.LastWriteUTC, writeUTC)
                                                                 .Set(r => r.FollowedUTC, writeUTC)
                                                                 .Set(r => r.LastReadUTC, writeUTC)
                                                                 .Set(r => r.UpdatedUTC, writeUTC)
                                                                 .Set(r => r.UpdatedByUserId, userId)
                                                                 .Set(r => r.Unread, false)
                                                                 .SetOnInsert(r => r.FeedId, feedId)
                                                                 .SetOnInsert(r => r.ContentEntityId, contentEntityId)
                                                                 .SetOnInsert(r => r.UserId, userId)
                                                                 .SetOnInsert(r => r.Deleted, false);

            await UpsertAsync(filter, update);
        }

        public async Task SetContentEntityMentionAsync(string userId, string feedId, string contentEntityId, DateTime mentionUTC)
        {
            var filter = Builders<UserContentEntityRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserContentEntityRecord>.Filter.Eq(r => r.ContentEntityId, contentEntityId) & Builders<UserContentEntityRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserContentEntityRecord>.Update.Set(r => r.LastMentionUTC, mentionUTC)
                                                                 .Set(r => r.FollowedUTC, mentionUTC)
                                                                 .Set(r => r.UpdatedUTC, mentionUTC)
                                                                 .Set(r => r.UpdatedByUserId, userId)
                                                                 .SetOnInsert(r => r.FeedId, feedId)
                                                                 .SetOnInsert(r => r.ContentEntityId, contentEntityId)
                                                                 .SetOnInsert(r => r.UserId, userId)
                                                                 .SetOnInsert(r => r.Deleted, false);

            await UpsertAsync(filter, update);
        }

        protected override UpdateDefinition<UserContentEntityRecord> GetDefaultUpdateDefinition(UserContentEntityRecord updatedEntity)
        {
            return Builders<UserContentEntityRecord>.Update.Set(r => r.Unread, updatedEntity.Unread)
                                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                                           .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<UserContentEntityRecord>();

            var searchIndexes = await ListIndexesAsync();
            if (!searchIndexes.Contains(INDEX_UNIQUE))
            {
                var builder = new IndexKeysDefinitionBuilder<UserContentEntityRecord>();
                var definition = builder.Ascending(u => u.UserId).Ascending(u => u.ContentEntityId);
                await coll.Indexes.CreateOneAsync(new CreateIndexModel<UserContentEntityRecord>(definition, new CreateIndexOptions { Unique = true, Name = INDEX_UNIQUE }));
            }
        }
    }
}