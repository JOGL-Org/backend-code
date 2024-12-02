using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class EmailRecordRepository : BaseRepository<EmailRecord>, IEmailRecordRepository
    {
        public EmailRecordRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "emailRecords";

        protected override UpdateDefinition<EmailRecord> GetDefaultUpdateDefinition(EmailRecord updatedEntity)
        {
            throw new NotImplementedException();
        }
    }
}