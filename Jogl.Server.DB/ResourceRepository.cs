using Jogl.Server.Data;
using Jogl.Server.Data.Util;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class ResourceRepository : BaseRepository<Resource>, IResourceRepository
    {
        public ResourceRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "resources";
        protected override IEnumerable<string> SearchFields
        {
            get
            {
                yield return nameof(Resource.Title);
                yield return nameof(Resource.Description);
            }
        }

        public override Expression<Func<Resource, object>> GetSort(SortKey key)
        {
            switch (key)
            {
                case SortKey.Alphabetical:
                    return e => e.Title;
                default:
                    return base.GetSort(key);
            }
        }

        protected override UpdateDefinition<Resource> GetDefaultUpdateDefinition(Resource updatedEntity)
        {
            return Builders<Resource>.Update.Set(e => e.Description, updatedEntity.Description)
                                        .Set(e => e.Title, updatedEntity.Title)
                                        .Set(e => e.Description, updatedEntity.Description)
                                        .Set(e => e.Data, updatedEntity.Data)
                                        .Set(e => e.DefaultVisibility, updatedEntity.DefaultVisibility)
                                        .Set(e => e.UserVisibility, updatedEntity.UserVisibility)
                                        .Set(e => e.CommunityEntityVisibility, updatedEntity.CommunityEntityVisibility)
                                        .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                        .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}