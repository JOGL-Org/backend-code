using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class FolderRepository : BaseRepository<Folder>, IFolderRepository
    {
        public FolderRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "folders";
        protected override Expression<Func<Folder, object>>[] SearchFields
        {
            get
            {
                return new Expression<Func<Folder, object>>[] { e => e.Name };
            }
        }

        public override Expression<Func<Folder, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.Alphabetical:
                    return e => e.Name;
                default:
                    return base.GetSort(key);
            }
        }

        protected override UpdateDefinition<Folder> GetDefaultUpdateDefinition(Folder updatedEntity)
        {
            return Builders<Folder>.Update.Set(e => e.Name, updatedEntity.Name)
                                            .Set(e => e.ParentFolderId, updatedEntity.ParentFolderId)
                                            .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                            .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                            .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }

        public override async Task InitializeAsync()
        {
            await EnsureExistsAsync();
            var coll = GetCollection<Folder>();

            var searchIndexes = await ListSearchIndexesAsync();
            if (!searchIndexes.Contains(INDEX_SEARCH))
                await coll.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(INDEX_SEARCH, new BsonDocument(new BsonDocument { { "storedSource", true }, { "mappings", new BsonDocument { { "dynamic", true } } } })));
        }
    }
}