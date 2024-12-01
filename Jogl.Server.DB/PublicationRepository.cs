using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class PublicationRepository : BaseRepository<Publication>, IPublicationRepository
    {
        public PublicationRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "publications";

        public override Expression<Func<Publication, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Publication, object>>[] { e => e.Title, e => e.Summary, e => e.Authors };
            }
        }

        public override Expression<Func<Publication, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.Date:
                    return e => e.Published;
                case SortKey.Alphabetical:
                    return e => e.Title;
                default:
                    return base.GetSort(key);
            }
        }


        protected override UpdateDefinition<Publication> GetDefaultUpdateDefinition(Publication updatedEntity)
        {
            return Builders<Publication>.Update.Set(e => e.DOI, updatedEntity.DOI)
                                               .Set(e => e.Title, updatedEntity.Title)
                                               .Set(e => e.Summary, updatedEntity.Summary)
                                               .Set(e => e.ExternalURL, updatedEntity.ExternalURL)
                                               .Set(e => e.ExternalFileURL, updatedEntity.ExternalFileURL)
                                               .Set(e => e.Authors, updatedEntity.Authors)
                                               .Set(e => e.Tags, updatedEntity.Tags)
                                               .Set(e => e.Published, updatedEntity.Published)
                                               .Set(e => e.LicenseURL, updatedEntity.LicenseURL)
                                               .Set(e => e.Journal, updatedEntity.Journal)
                                               .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                               .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }

        protected override UpdateDefinition<Publication> GetDefaultUpsertDefinition(Publication updatedEntity)
        {
            return Builders<Publication>.Update.Set(e => e.DOI, updatedEntity.DOI)
                                               .Set(e => e.Title, updatedEntity.Title)
                                               .Set(e => e.Summary, updatedEntity.Summary)
                                               .Set(e => e.ExternalURL, updatedEntity.ExternalURL)
                                               .Set(e => e.ExternalFileURL, updatedEntity.ExternalFileURL)
                                               .Set(e => e.Authors, updatedEntity.Authors)
                                               .Set(e => e.Tags, updatedEntity.Tags)
                                               .Set(e => e.Published, updatedEntity.Published)
                                               .Set(e => e.LicenseURL, updatedEntity.LicenseURL)
                                               .Set(e => e.Journal, updatedEntity.Journal)
                                               .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                               .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                               .SetOnInsert(e => e.ExternalSystem, updatedEntity.ExternalSystem)
                                               .SetOnInsert(e => e.ExternalID, updatedEntity.ExternalID)
                                               .SetOnInsert(e => e.CreatedUTC, updatedEntity.CreatedUTC)
                                               .SetOnInsert(e => e.CreatedByUserId, updatedEntity.CreatedByUserId);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<Publication>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));

            var indexes = await ListIndexesAsync();
            if (!indexes.Contains(INDEX_UNIQUE))
            {
                var builder = new IndexKeysDefinitionBuilder<Publication>();
                var definition = builder.Ascending(u => u.ExternalID).Ascending(u => u.ExternalSystem);
                await coll.Indexes.CreateOneAsync(new CreateIndexModel<Publication>(definition, new CreateIndexOptions { Unique = true, Name = INDEX_UNIQUE }));
            }
        }
    }
}