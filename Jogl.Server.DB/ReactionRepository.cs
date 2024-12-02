using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class ReactionRepository : BaseRepository<Reaction>, IReactionRepository
    {
        public ReactionRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "reactions";

        protected override UpdateDefinition<Reaction> GetDefaultUpdateDefinition(Reaction updatedEntity)
        {
            return Builders<Reaction>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                            .Set(e => e.Key, updatedEntity.Key)
                                            .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}