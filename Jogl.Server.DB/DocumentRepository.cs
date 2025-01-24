using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class DocumentRepository : BaseRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "documents";
   
        protected override IEnumerable<string> SearchFields
        {
            get
            {
                yield return nameof(Document.Name);
                yield return nameof(Document.Filename);
                yield return nameof(Document.Description);
                yield return nameof(Document.URL);
            }
        }

        public override Expression<Func<Document, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.Alphabetical:
                    return e => e.Name;
                default:
                    return base.GetSort(key);
            }
        }

        protected override UpdateDefinition<Document> GetDefaultUpdateDefinition(Document updatedEntity)
        {
            return Builders<Document>.Update.Set(e => e.Name, updatedEntity.Name)
                                            .Set(e => e.FolderId, updatedEntity.FolderId)
                                            .Set(e => e.URL, updatedEntity.URL)
                                            .Set(e => e.Description, updatedEntity.Description)
                                            .Set(e => e.ImageId, updatedEntity.ImageId)
                                            .Set(e => e.DefaultVisibility, updatedEntity.DefaultVisibility)
                                            .Set(e => e.UserVisibility, updatedEntity.UserVisibility)
                                            .Set(e => e.CommunityEntityVisibility, updatedEntity.CommunityEntityVisibility)
                                            .Set(e => e.Status, updatedEntity.Status)
                                            .Set(e => e.Keywords, updatedEntity.Keywords)
                                            .Set(e => e.UserIds, updatedEntity.UserIds)
                                            .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                            .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                            .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<Document>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}