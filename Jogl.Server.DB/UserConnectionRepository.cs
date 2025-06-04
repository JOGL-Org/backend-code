using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class UserConnectionRepository : BaseRepository<UserConnection>, IUserConnectionRepository
    {
        public UserConnectionRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "userConnections";

        protected override UpdateDefinition<UserConnection> GetDefaultUpdateDefinition(UserConnection updatedEntity)
        {
            return Builders<UserConnection>.Update.Set(r => r.Status, updatedEntity.Status)
                                                  .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                  .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                                  .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}