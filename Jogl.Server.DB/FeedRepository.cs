using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        public FeedRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "feed";

        protected override UpdateDefinition<Feed> GetDefaultUpdateDefinition(Feed updatedEntity)
        {
            return Builders<Feed>.Update.Set(e => e.Type, updatedEntity.Type)//TODO remove after migration
                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }
    }
}