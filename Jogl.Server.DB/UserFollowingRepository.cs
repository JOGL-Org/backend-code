using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Jogl.Server.DB
{
    public class UserFollowingRepository : BaseRepository<UserFollowing>, IUserFollowingRepository
    {
        public UserFollowingRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string CollectionName => "userFollow";

        public UserFollowing GetFollowing(string userIdFrom, string userIdTo)
        {
            var coll = GetCollection<UserFollowing>();
            return coll.Find(e => e.UserIdFrom == userIdFrom && e.UserIdTo == userIdTo && !e.Deleted).FirstOrDefault();
        }

        public List<UserFollowing> ListForFolloweds(IEnumerable<string> ids)
        {
            var coll = GetCollection<UserFollowing>();
            var filterBuilder = Builders<UserFollowing>.Filter;
            var filter = filterBuilder.In(e => e.UserIdTo, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        public List<UserFollowing> ListForFollowers(IEnumerable<string> ids)
        {
            var coll = GetCollection<UserFollowing>();
            var filterBuilder = Builders<UserFollowing>.Filter;
            var filter = filterBuilder.In(e => e.UserIdFrom, ids) & filterBuilder.Eq(e => e.Deleted, false);
            return coll.Find(filter).ToList();
        }

        protected override UpdateDefinition<UserFollowing> GetDefaultUpdateDefinition(UserFollowing updatedEntity)
        {
            return Builders<UserFollowing>.Update.Set(e => e.UpdatedUTC, updatedEntity.UpdatedUTC)
                                                 .Set(e => e.UpdatedByUserId, updatedEntity.UpdatedByUserId) .Set(e => e.LastActivityUTC, updatedEntity.LastActivityUTC);
        }
    }
}