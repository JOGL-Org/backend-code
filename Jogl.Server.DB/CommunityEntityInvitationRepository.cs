using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class CommunityEntityInvitationRepository : BaseRepository<CommunityEntityInvitation>, ICommunityEntityInvitationRepository
    {
        public CommunityEntityInvitationRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "communityEntityInvitations";


        protected override UpdateDefinition<CommunityEntityInvitation> GetDefaultUpdateDefinition(CommunityEntityInvitation updatedEntity)
        {
            return Builders<CommunityEntityInvitation>.Update.Set(e => e.Status, updatedEntity.Status)
                                                             .Set(e => e.SourceCommunityEntityType, updatedEntity.SourceCommunityEntityType) //TODO remove after migration
                                                             .Set(e => e.TargetCommunityEntityType, updatedEntity.TargetCommunityEntityType) //TODO remove after migration
                                                             .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                             .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}