using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class InvitationRepository : BaseRepository<Invitation>, IInvitationRepository
    {
        public InvitationRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "invitations";

        protected override UpdateDefinition<Invitation> GetDefaultUpdateDefinition(Invitation updatedEntity)
        {
            return Builders<Invitation>.Update.Set(e => e.Status, updatedEntity.Status)
                                              .Set(e => e.CommunityEntityType, updatedEntity.CommunityEntityType) //TODO remove after migration
                                              .Set(e => e.InviteeUserId, updatedEntity.InviteeUserId)
                                              .Set(e => e.InviteeEmail, updatedEntity.InviteeEmail)
                                              .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                              .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}