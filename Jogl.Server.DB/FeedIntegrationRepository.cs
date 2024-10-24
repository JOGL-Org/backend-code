﻿using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class FeedIntegrationRepository : BaseRepository<FeedIntegration>, IFeedIntegrationRepository
    {
        public FeedIntegrationRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override Expression<Func<FeedIntegration, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<FeedIntegration, object>>[] { e => e.SourceUrl};
            }
        }

        protected override string CollectionName => "feedIntegrations";

        protected override UpdateDefinition<FeedIntegration> GetDefaultUpdateDefinition(FeedIntegration updatedEntity)
        {
            return Builders<FeedIntegration>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                          .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            var coll = GetCollection<FeedIntegration>();
            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}