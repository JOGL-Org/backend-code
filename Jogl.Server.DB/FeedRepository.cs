using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        public FeedRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
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