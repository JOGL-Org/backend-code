using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class WaitlistRecordRepository : BaseRepository<WaitlistRecord>, IWaitlistRecordRepository
    {
        public WaitlistRecordRepository(IConfiguration configuration) : base(configuration)
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