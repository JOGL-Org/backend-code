using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class ConversationRepository : BaseRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(IConfiguration configuration, IOperationContext context = null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "conversations";

        protected override UpdateDefinition<Conversation> GetDefaultUpdateDefinition(Conversation updatedEntity)
        {
            return Builders<Conversation>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}