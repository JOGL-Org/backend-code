using Jogl.Server.Data;
using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class CommunityEntityFollowingRepository : BaseRepository<CommunityEntityFollowing>, ICommunityEntityFollowingRepository
    {
        public CommunityEntityFollowingRepository(IConfiguration configuration, IOperationContext context=null) : base(configuration, context)
        {
        }

        protected override string CollectionName => "communityEntityFollow";

        public CommunityEntityFollowing GetFollowing(string userIdFrom, string communityEntityId)
        {
            var coll = GetCollection<CommunityEntityFollowing>();
            return coll.Find(e => e.UserIdFrom == userIdFrom && e.CommunityEntityId == communityEntityId && !e.Deleted).FirstOrDefault();
        }

        public List<CommunityEntityFollowing> ListForFolloweds(IEnumerable<string> ids)
        {
            var coll = GetCollection<CommunityEntityFollowing>();
            var filterBuilder = Builders<CommunityEntityFollowing>.Filter;
            var filter = filterBuilder.In(e => e.CommunityEntityId, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public List<CommunityEntityFollowing> ListForFollowers(IEnumerable<string> ids)
        {
            var coll = GetCollection<CommunityEntityFollowing>();
            var filterBuilder = Builders<CommunityEntityFollowing>.Filter;
            var filter = filterBuilder.In(e => e.UserIdFrom, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<CommunityEntityFollowing> GetDefaultUpdateDefinition(CommunityEntityFollowing updatedEntity)
        {
            return Builders<CommunityEntityFollowing>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                            .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}