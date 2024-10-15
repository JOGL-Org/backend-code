using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class MentionRepository : BaseRepository<Mention>, IMentionRepository
    {
        public MentionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "mention";

        protected override UpdateDefinition<Mention> GetDefaultUpdateDefinition(Mention updatedEntity)
        {
            return Builders<Mention>.Update.Set(e => e.EntityTitle, updatedEntity.EntityTitle)
                                           .Set(e => e.Unread, updatedEntity.Unread)
                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public async Task SetMentionReadAsync(Mention mention)
        {
            await UpdateAsync(mention.Id.ToString(), Builders<Mention>.Update.Set(e => e.Unread, false)
                                                                             .Set(e => e.UpdatedUTC, DateTime.UtcNow)
                                                                             .Set(e => e.UpdatedByUserId, mention.UpdatedByUserId));
        }
    }
}