using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class SkillRepository : BaseRepository<TextValue>, ISkillRepository
    {
        public SkillRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
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