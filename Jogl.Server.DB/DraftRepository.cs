using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class DraftRepository : BaseRepository<Draft>, IDraftRepository
    {
        public DraftRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "drafts";

        protected override UpdateDefinition<Draft> GetDefaultUpdateDefinition(Draft updatedEntity)
        {
            return Builders<Draft>.Update.Set(e => e.Text, updatedEntity.Text)
                                         .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                         .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public async Task SetDraftAsync(string entityId, string userId, string text, DateTime updatedUTC)
        {
            var filter = Builders<Draft>.Filter.Eq(r => r.EntityId, entityId) & Builders<Draft>.Filter.Eq(r => r.UserId, userId);
            var update = Builders<Draft>.Update.Set(r => r.Text, text)
                                               .Set(r => r.UpdatedByUserId, userId)
                                               .Set(r => r.UpdatedUTC, updatedUTC)
                                               .SetOnInsert(r => r.EntityId, entityId)
                                               .SetOnInsert(r => r.UserId, userId)
                                               .SetOnInsert(r => r.CreatedByUserId, userId)
                                               .SetOnInsert(r => r.CreatedUTC, updatedUTC)
                                               .SetOnInsert(r => r.Deleted, false);

            await UpsertAsync(filter, update);
        }
    }
}