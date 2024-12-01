using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class MembershipRepository : BaseRepository<Membership>, IMembershipRepository
    {
        public MembershipRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "memberships";

        public Membership Get(string entityId, string userId)
        {
            var coll = GetCollection<Membership>();
            return coll.Find(e => e.CommunityEntityId == entityId && e.UserId == userId && !e.Deleted).FirstOrDefault();
        }

        public List<Membership> ListForUsers(IEnumerable<string> ids)
        {
            var coll = GetCollection<Membership>();
            var filterBuilder = Builders<Membership>.Filter;
            var filter = filterBuilder.In(e => e.UserId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public List<Membership> ListForCommunityEntities(IEnumerable<string> ids)
        {
            var coll = GetCollection<Membership>();
            var filterBuilder = Builders<Membership>.Filter;
            var filter = filterBuilder.In(e => e.CommunityEntityId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<Membership> GetDefaultUpdateDefinition(Membership updatedEntity)
        {
            return Builders<Membership>.Update.Set(e => e.AccessLevel, updatedEntity.AccessLevel)
                                              .Set(e => e.Contribution, updatedEntity.Contribution)
                                              .Set(e => e.CommunityEntityType, updatedEntity.CommunityEntityType) //TODO remove after migration
                                              .Set(e => e.OnboardedUTC, updatedEntity.OnboardedUTC)
                                              .Set(e => e.Labels, updatedEntity.Labels)
                                              .Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                              .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId).Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}