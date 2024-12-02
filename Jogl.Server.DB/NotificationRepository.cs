using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "notifications";

        protected override UpdateDefinition<Notification> GetDefaultUpdateDefinition(Notification updatedEntity)
        {
            return Builders<Notification>.Update.Set(e => e.Actioned, updatedEntity.Actioned)
                                                .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}