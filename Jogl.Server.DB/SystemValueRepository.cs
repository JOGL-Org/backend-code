using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class SystemValueRepository : BaseRepository<SystemValue>, ISystemValueRepository
    {
        public SystemValueRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "systemValues";

        protected override UpdateDefinition<SystemValue> GetDefaultUpdateDefinition(SystemValue updatedEntity)
        {
            return Builders<SystemValue>.Update.Set(e => e.Value, updatedEntity.Value)
                                               .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                               .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        protected override UpdateDefinition<SystemValue> GetDefaultUpsertDefinition(SystemValue updatedEntity)
        {
            return Builders<SystemValue>.Update.Set(e => e.Value, updatedEntity.Value)
                                               .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                               .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                               .SetOnInsert(e => e.Key, updatedEntity.Key)
                                               .SetOnInsert(e => e.Deleted, false);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<SystemValue>();

            var searchIndexes = await ListIndexesAsync();
            if (!searchIndexes.Contains(INDEX_UNIQUE))
            {
                var builder = new IndexKeysDefinitionBuilder<SystemValue>();
                var definition = builder.Ascending(v => v.Key);
                await coll.Indexes.CreateOneAsync(new CreateIndexModel<SystemValue>(definition, new CreateIndexOptions { Unique = true, Name = INDEX_UNIQUE }));
            }
        }
    }
}