using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class EmailRecordRepository : BaseRepository<EmailRecord>, IEmailRecordRepository
    {
        public EmailRecordRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "emailRecords";

        protected override UpdateDefinition<EmailRecord> GetDefaultUpdateDefinition(EmailRecord updatedEntity)
        {
            throw new NotImplementedException();
        }
    }
}