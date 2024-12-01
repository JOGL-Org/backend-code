using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class FeedIntegrationRepository : BaseRepository<FeedIntegration>, IFeedIntegrationRepository
    {
        public FeedIntegrationRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override Expression<Func<FeedIntegration, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<FeedIntegration, object>>[] { e => e.SourceId, e => e.SourceUrl };
            }
        }

        protected override Expression<Func<FeedIntegration, string>> AutocompleteField => (e) => e.SourceId;

        protected override string CollectionName => "feedIntegrations";

        protected override UpdateDefinition<FeedIntegration> GetDefaultUpdateDefinition(FeedIntegration updatedEntity)
        {
            return Builders<FeedIntegration>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                          .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<FeedIntegration>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}