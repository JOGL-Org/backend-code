using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class InterfaceUserRepository : BaseRepository<InterfaceUser>, IInterfaceUserRepository
    {
        public InterfaceUserRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "interfaceUser";

        protected override UpdateDefinition<InterfaceUser> GetDefaultUpdateDefinition(InterfaceUser updatedEntity)
        {
            return Builders<InterfaceUser>.Update.Set(e => e.OnboardingStatus, updatedEntity.OnboardingStatus)
                                                 .Set(e => e.UserId, updatedEntity.UserId)
                                                 .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                 .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }
    }
}