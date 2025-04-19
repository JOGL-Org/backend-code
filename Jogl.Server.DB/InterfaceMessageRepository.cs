using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class InterfaceMessageRepository : BaseRepository<InterfaceMessage>, IInterfaceMessageRepository
    {
        public InterfaceMessageRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "interfaceMessage";

        protected override UpdateDefinition<InterfaceMessage> GetDefaultUpdateDefinition(InterfaceMessage updatedEntity)
        {
            return Builders<InterfaceMessage>.Update.Set(e => e.Tag, updatedEntity.Tag)
                                                    .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                    .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId);
        }
    }
}