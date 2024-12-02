using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class InvitationRepository : BaseRepository<Invitation>, IInvitationRepository
    {
        public InvitationRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "invitations";

        protected override UpdateDefinition<Invitation> GetDefaultUpdateDefinition(Invitation updatedEntity)
        {
            return Builders<Invitation>.Update.Set(e => e.Status, updatedEntity.Status)
                                              .Set(e => e.InviteeUserId, updatedEntity.InviteeUserId)
                                              .Set(e => e.InviteeEmail, updatedEntity.InviteeEmail)
                                              .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                              .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}