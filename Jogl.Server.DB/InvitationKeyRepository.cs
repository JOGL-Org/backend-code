using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class InvitationKeyRepository : BaseRepository<InvitationKey>, IInvitationKeyRepository
    {
        public InvitationKeyRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "invitationKeys";

        protected override UpdateDefinition<InvitationKey> GetDefaultUpdateDefinition(InvitationKey updatedEntity)
        {
            return Builders<InvitationKey>.Update
                                              .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                              .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                              .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<InvitationKey>();

            var searchIndexes = await ListIndexesAsync();
            if (!searchIndexes.Contains(INDEX_UNIQUE))
            {
                var builder = new IndexKeysDefinitionBuilder<InvitationKey>();
                var definition = builder.Ascending(u => u.InviteKey);
                await coll.Indexes.CreateOneAsync(new CreateIndexModel<InvitationKey>(definition, new CreateIndexOptions { Unique = true, Name = INDEX_UNIQUE }));
            }
        }
    }
}