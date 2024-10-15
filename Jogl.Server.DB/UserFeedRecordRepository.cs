using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class UserFeedRecordRepository : BaseRepository<UserFeedRecord>, IUserFeedRecordRepository
    {
        public UserFeedRecordRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "userFeedRecords";

        public async Task SetFeedListedAsync(string userId, string feedId, DateTime listedUTC)
        {
            var filter = Builders<UserFeedRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserFeedRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserFeedRecord>.Update.Set(r => r.LastListedUTC, listedUTC)
                                                        .Set(r => r.UpdatedUTC, listedUTC)
                                                        .Set(r => r.UpdatedByUserId, userId)
                                                        .SetOnInsert(r => r.FeedId, feedId)
                                                        .SetOnInsert(r => r.UserId, userId)
                                                        .SetOnInsert(r => r.Deleted, false)
                                                        .SetOnInsert(r => r.CreatedUTC, listedUTC)
                                                        .SetOnInsert(r => r.CreatedByUserId, userId);

            await UpsertAsync(filter, update);
        }

        public async Task SetFeedMentionAsync(string userId, string feedId, DateTime mentionUTC)
        {
            var filter = Builders<UserFeedRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserFeedRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserFeedRecord>.Update.Set(r => r.LastMentionUTC, mentionUTC)
                                                        .Set(r => r.UpdatedUTC, mentionUTC)
                                                        .Set(r => r.UpdatedByUserId, userId)
                                                        .SetOnInsert(r => r.FeedId, feedId)
                                                        .SetOnInsert(r => r.UserId, userId)
                                                        .SetOnInsert(r => r.Deleted, false)
                                                        .SetOnInsert(r => r.CreatedUTC, mentionUTC)
                                                        .SetOnInsert(r => r.CreatedByUserId, userId);


            await UpsertAsync(filter, update);
        }

        public async Task SetFeedReadAsync(string userId, string feedId, DateTime readUTC)
        {
            var filter = Builders<UserFeedRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserFeedRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserFeedRecord>.Update.Set(r => r.LastReadUTC, readUTC)
                                                        .Set(r => r.UpdatedUTC, readUTC)
                                                        .Set(r => r.UpdatedByUserId, userId)
                                                        .SetOnInsert(r => r.FeedId, feedId)
                                                        .SetOnInsert(r => r.UserId, userId)
                                                        .SetOnInsert(r => r.Deleted, false)
                                                        .SetOnInsert(r => r.CreatedUTC, readUTC)
                                                        .SetOnInsert(r => r.CreatedByUserId, userId);

            await UpsertAsync(filter, update);
        }

        public async Task SetFeedWrittenAsync(string userId, string feedId, DateTime writeUTC)
        {
            var filter = Builders<UserFeedRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserFeedRecord>.Filter.Eq(r => r.FeedId, feedId);
            var update = Builders<UserFeedRecord>.Update.Set(r => r.LastWriteUTC, writeUTC)
                                                        .Set(r => r.LastReadUTC, writeUTC)
                                                        .Set(r => r.UpdatedUTC, writeUTC)
                                                        .Set(r => r.UpdatedByUserId, userId)
                                                        .SetOnInsert(r => r.FeedId, feedId)
                                                        .SetOnInsert(r => r.UserId, userId)
                                                        .SetOnInsert(r => r.Deleted, false)
                                                        .SetOnInsert(r => r.CreatedUTC, writeUTC)
                                                        .SetOnInsert(r => r.CreatedByUserId, userId);

            await UpsertAsync(filter, update);
        }

        protected override UpdateDefinition<UserFeedRecord> GetDefaultUpdateDefinition(UserFeedRecord updatedEntity)
        {
            return Builders<UserFeedRecord>.Update.Set(e => e.Muted, updatedEntity.Muted)
                                                  .Set(e => e.Starred, updatedEntity.Starred)
                                            .Set(e => e.FeedId, updatedEntity.FeedId)
                                                  .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                  .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                                  .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public async Task DeleteAsync(string userId, string feedId)
        {
            var filter = Builders<UserFeedRecord>.Filter.Eq(r => r.UserId, userId) & Builders<UserFeedRecord>.Filter.Eq(r => r.FeedId, feedId);
            await DeleteAsync(filter);
        }

        public override async Task InitializeAsync()
        {
            var coll = GetCollection<UserFeedRecord>();
            var searchIndexes = await ListIndexesAsync();
            if (!searchIndexes.Contains(INDEX_UNIQUE))
            {
                var builder = new IndexKeysDefinitionBuilder<UserFeedRecord>();
                var definition = builder.Ascending(u => u.UserId).Ascending(u => u.FeedId);
                await coll.Indexes.CreateOneAsync(new CreateIndexModel<UserFeedRecord>(definition, new CreateIndexOptions { Unique = true, Name = INDEX_UNIQUE }));
            }
        }
    }
}