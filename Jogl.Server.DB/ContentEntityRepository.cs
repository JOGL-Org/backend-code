using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class ContentEntityRepository : BaseRepository<ContentEntity>, IContentEntityRepository
    {
        public ContentEntityRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "contentEntity";

        public List<ContentEntity> List(IEnumerable<string> feedIds, Expression<Func<ContentEntity, bool>> filter, int page, int pageSize)
        {
            var coll = GetCollection<ContentEntity>();
            var filterBuilder = Builders<ContentEntity>.Filter;
            var filterObject = filterBuilder.In(e => e.FeedId, feedIds) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filterObject)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
        }

        public List<ContentEntity> List(IEnumerable<string> feedIds)
        {
            var coll = GetCollection<ContentEntity>();
            var filterBuilder = Builders<ContentEntity>.Filter;
            var filterObject = filterBuilder.In(e => e.FeedId, feedIds) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filterObject).ToList();
        }

        protected override UpdateDefinition<ContentEntity> GetDefaultUpdateDefinition(ContentEntity updatedEntity)
        {
            return Builders<ContentEntity>.Update.Set(e => e.Text, updatedEntity.Text)
                                                 .Set(e => e.Status, updatedEntity.Status)
                                                 .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                 .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}