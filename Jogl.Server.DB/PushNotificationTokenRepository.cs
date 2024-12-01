using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class PushNotificationTokenRepository : BaseRepository<PushNotificationToken>, IPushNotificationTokenRepository
    {
        public PushNotificationTokenRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "pushNotifications";

        public async Task UpsertTokenAsync(string userId, string token, DateTime date)
        {
            var filter = Builders<PushNotificationToken>.Filter.Eq(r => r.UserId, userId) & Builders<PushNotificationToken>.Filter.Eq(r => r.Token, token);
            var update = Builders<PushNotificationToken>.Update.Set(r => r.UpdatedUTC, date)
                                                               .Set(r => r.UpdatedByUserId, userId)
                                                               .SetOnInsert(r => r.UserId, userId)
                                                               .SetOnInsert(r => r.Token, token)
                                                               .SetOnInsert(r => r.Deleted, false)
                                                               .SetOnInsert(r => r.CreatedUTC, date)
                                                               .SetOnInsert(r => r.CreatedByUserId, userId);

            await UpsertAsync(filter, update);
        }

        protected override UpdateDefinition<PushNotificationToken> GetDefaultUpdateDefinition(PushNotificationToken updatedEntity)
        {
            return Builders<PushNotificationToken>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                         .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId)
                                                         .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}