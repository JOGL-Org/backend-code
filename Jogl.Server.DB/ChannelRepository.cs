using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class ChannelRepository : BaseRepository<Channel>, IChannelRepository
    {
        public ChannelRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "channels";

        protected override Expression<Func<Channel, string>> AutocompleteField => e => e.Title;
        protected override Expression<Func<Channel, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Channel, object>>[] { e => e.Title, e => e.Description };
            }
        }

        protected override Expression<Func<Channel, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.CreatedDate:
                    return (e) => e.CreatedUTC;
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

        protected override UpdateDefinition<Channel> GetDefaultUpdateDefinition(Channel updatedEntity)
        {
            return Builders<Channel>.Update.Set(e => e.Title, updatedEntity.Title)
                                           .Set(e => e.Description, updatedEntity.Description)
                                           .Set(e => e.IconKey, updatedEntity.IconKey)
                                           .Set(e => e.Settings, updatedEntity.Settings)
                                           .Set(e => e.Visibility, updatedEntity.Visibility)
                                           .Set(e => e.AutoJoin, updatedEntity.AutoJoin)
                                           .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                           .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public async override Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<Channel>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));

            if (!searchIndexes.Contains(INDEX_AUTOCOMPLETE))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_AUTOCOMPLETE, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", false }, { "fields", new BsonDocument { { nameof(Channel.Title), new BsonDocument { { "tokenization", "nGram" }, { "type", "autocomplete" } } } } } } } })));
        }
    }
}