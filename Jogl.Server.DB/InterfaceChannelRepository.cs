using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class InterfaceChannelRepository : BaseRepository<InterfaceChannel>, IInterfaceChannelRepository
    {
        public InterfaceChannelRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "interfaceChannel";

        protected override UpdateDefinition<InterfaceChannel> GetDefaultUpdateDefinition(InterfaceChannel updatedEntity)
        {
            return Builders<InterfaceChannel>.Update.Set(e => e.Key, updatedEntity.Key)
                                                    .Set(e => e.NodeId, updatedEntity.NodeId)
                                                    .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                    .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }
    }
}