using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class WaitlistRecordRepository : BaseRepository<WaitlistRecord>, IWaitlistRecordRepository
    {
        public WaitlistRecordRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "waitlist";

        protected override UpdateDefinition<WaitlistRecord> GetDefaultUpdateDefinition(WaitlistRecord updatedEntity)
        {
            return Builders<WaitlistRecord>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                  .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}