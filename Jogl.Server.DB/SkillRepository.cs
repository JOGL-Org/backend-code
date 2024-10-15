using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class SkillRepository : BaseRepository<TextValue>, ISkillRepository
    {
        public SkillRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "skills";

      
        protected override UpdateDefinition<TextValue> GetDefaultUpdateDefinition(TextValue updatedEntity)
        {
            return Builders<TextValue>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                             .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}