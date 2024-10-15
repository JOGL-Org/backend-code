using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class FolderRepository : BaseRepository<Folder>, IFolderRepository
    {
        public FolderRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "folders";

        protected override Expression<Func<Folder, object>> GetSort(SortKey key)
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
                    return (e) => e.Name;
                default:
                    return null;
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
    }
}