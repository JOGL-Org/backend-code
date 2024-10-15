using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class EntityScoreRepository : BaseRepository<EntityScore>, IEntityScoreRepository
    {
        public EntityScoreRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "entityScores";

        protected override UpdateDefinition<EntityScore> GetDefaultUpdateDefinition(EntityScore updatedEntity)
        {
            return Builders<EntityScore>.Update.Set(e => e.Score, updatedEntity.Score)
                                               .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                               .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }
    }
}