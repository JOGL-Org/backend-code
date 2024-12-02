using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class UserVerificationCodeRepository : BaseRepository<UserVerificationCode>, IUserVerificationCodeRepository
    {
        public UserVerificationCodeRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "userVerificationCodes";

        public UserVerificationCode GetForCode(string code)
        {
            var coll = GetCollection<UserVerificationCode>();
            return coll.Find(e => e.Code == code && !e.Deleted).FirstOrDefault();
        }

        protected override UpdateDefinition<UserVerificationCode> GetDefaultUpdateDefinition(UserVerificationCode updatedEntity)
        {
            return Builders<UserVerificationCode>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                        .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}