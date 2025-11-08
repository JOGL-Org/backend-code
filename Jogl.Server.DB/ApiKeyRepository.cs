using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class ApiKeyRepository : BaseRepository<ApiKey>, IApiKeyRepository
    {
        public ApiKeyRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "apiKeys";

        protected override UpdateDefinition<ApiKey> GetDefaultUpdateDefinition(ApiKey updatedEntity)
        {
            return Builders<ApiKey>.Update.Set(e => e.NodeId, updatedEntity.NodeId)
                                          .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                          .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                          .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}