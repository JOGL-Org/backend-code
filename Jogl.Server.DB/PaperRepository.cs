using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class PaperRepository : BaseRepository<Paper>, IPaperRepository
    {
        public PaperRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override Expression<Func<Paper, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Paper, object>>[] { e => e.Title, e => e.Summary, e => e.Authors };
            }
        }

        protected override Expression<Func<Paper, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
                case SortKey.LastActivity:
                    return (e) => e.LastActivityUTC;
                case SortKey.Date:
                    return (e) => e.PublicationDate;
                case SortKey.Alphabetical:
                    return (e) => e.Title;
                default:
                    return null;
            }
        }

        protected override string CollectionName => "papers";

        public List<Paper> ListForExternalIds(IEnumerable<string> ids)
        {
            var coll = GetCollection<Paper>();
            var filterBuilder = Builders<Paper>.Filter;
            var filter = filterBuilder.In(e => e.ExternalId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Paper> GetDefaultUpdateDefinition(Paper updatedEntity)
        {
            return Builders<Paper>.Update/*.Set(e => e.Title, updatedEntity.Title)*/
                                         //.Set(e => e.Summary, updatedEntity.Summary)
                                         //.Set(e => e.PublicationDate, updatedEntity.PublicationDate)
                                         //.Set(e => e.Authors, updatedEntity.Authors)
                                         //.Set(e => e.ExternalId, updatedEntity.ExternalId)
                                         //.Set(e => e.TagData, updatedEntity.TagData)
                                         //.Set(e => e.Status, updatedEntity.Status)
                                         //.Set(e => e.UserIds, updatedEntity.UserIds)
                                         //.Set(e => e.Journal, updatedEntity.Journal)
                                         //.Set(e => e.OpenAccessPdfUrl, updatedEntity.OpenAccessPdfUrl)
                                         .Set(e => e.FeedIds, updatedEntity.FeedIds)
                                         .Set(e => e.FeedId, updatedEntity.FeedId)
                                         .Set(e => e.DefaultVisibility, updatedEntity.DefaultVisibility)
                                         .Set(e => e.UserVisibility, updatedEntity.UserVisibility)
                                         .Set(e => e.CommunityEntityVisibility, updatedEntity.CommunityEntityVisibility)
                                         .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                         .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                         .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<Paper>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}