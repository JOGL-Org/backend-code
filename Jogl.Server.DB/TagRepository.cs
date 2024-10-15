using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Tag = Jogl.Server.Data.Tag;

namespace Jogl.Server.DB
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "tags";

      
        protected override UpdateDefinition<Tag> GetDefaultUpdateDefinition(Tag updatedEntity)
        {
            return Builders<Tag>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                       .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}