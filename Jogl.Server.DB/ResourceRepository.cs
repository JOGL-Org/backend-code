using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class ResourceRepository : BaseRepository<Resource>, IResourceRepository
    {
        public ResourceRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "resources";
        protected override Expression<Func<Resource, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Resource, object>>[] { e => e.Title, e => e.Description };
            }
        }

        public override Expression<Func<Resource, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.Alphabetical:
                    return e => e.Title;
                default:
                    return base.GetSort(key);
            }
        }

        public List<Resource> ListForFeeds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Resource>();
            var filterBuilder = Builders<Resource>.Filter;
            var filter = filterBuilder.In(e => e.FeedId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Resource> GetDefaultUpdateDefinition(Resource updatedEntity)
        {
            return Builders<Resource>.Update.Set(e => e.Description, updatedEntity.Description)
                                        .Set(e => e.Title, updatedEntity.Title)
                                        .Set(e => e.Description, updatedEntity.Description)
                                        .Set(e => e.ImageId, updatedEntity.ImageId)
                                        .Set(e => e.Type, updatedEntity.Type)
                                        .Set(e => e.Condition, updatedEntity.Condition)
                                        .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                        .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}