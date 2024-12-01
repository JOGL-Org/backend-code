using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class NeedRepository : BaseRepository<Need>, INeedRepository
    {
        public NeedRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "needs";

        protected override Expression<Func<Need, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Need, object>>[] { e => e.Title, e => e.Description, e => e.Skills };
            }
        }

        public override Expression<Func<Need, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
                case SortKey.LastActivity:
                    return (e) => e.LastActivityUTC;
                case SortKey.Date:
                    return (e) => e.EndDate;
                case SortKey.Alphabetical:
                    return (e) => e.Title;
                default:
                    return null;
            }
        }

        public List<Need> ListForEntityIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Need>();
            var filterBuilder = Builders<Need>.Filter;
            var filter = filterBuilder.In(e => e.EntityId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public List<Need> ListForUsers(IEnumerable<string> ids)
        {
            var coll = GetCollection<Need>();
            var filterBuilder = Builders<Need>.Filter;
            var filter = filterBuilder.In(e => e.CreatedByUserId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Need> GetDefaultUpdateDefinition(Need updatedEntity)
        {
            return Builders<Need>.Update.Set(e => e.Description, updatedEntity.Description)
                                        .Set(e => e.Title, updatedEntity.Title)
                                        .Set(e => e.EndDate, updatedEntity.EndDate)
                                        .Set(e => e.Interests, updatedEntity.Interests)
                                        .Set(e => e.Skills, updatedEntity.Skills)
                                        .Set(e => e.Type, updatedEntity.Type)
                                        .Set(e => e.DefaultVisibility, updatedEntity.DefaultVisibility)
                                        .Set(e => e.UserVisibility, updatedEntity.UserVisibility)
                                        .Set(e => e.CommunityEntityVisibility, updatedEntity.CommunityEntityVisibility)
                                        .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                        .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<Need>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}