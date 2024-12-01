using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public abstract class CommunityEntityRepository<T> : BaseRepository<T> where T : CommunityEntity
    {
        protected CommunityEntityRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override Expression<Func<T, string>> AutocompleteField => e => e.Title;
        protected override Expression<Func<T, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<T, object>>[] { e => e.Title, e => e.ShortDescription, e => e.Description, e => e.Keywords };
            }
        }

        protected override Expression<Func<T, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
                case SortKey.RecentlyOpened:
                    return (e) => e.LastOpenedUTC;
                case SortKey.LastActivity:
                    return (e) => e.LastActivityUTC;
                case SortKey.Date:
                    return (e) => e.CreatedUTC;
                case SortKey.Alphabetical:
                    return (e) => e.Title;
                default:
                    return null;
            }
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<T>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));

            if (!searchIndexes.Contains(INDEX_AUTOCOMPLETE))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_AUTOCOMPLETE, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", false }, { "fields", new BsonDocument { { nameof(CommunityEntity.Title), new BsonDocument { { "tokenization", "nGram" }, { "type", "autocomplete" } } } } } } } })));
        }
    }
}