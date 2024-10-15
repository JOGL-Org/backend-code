using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IUserFollowingRepository : IRepository<UserFollowing>
    {
        UserFollowing GetFollowing(string userIdFrom, string userIdTo);
        List<UserFollowing> ListForFollowers(IEnumerable<string> ids);
        List<UserFollowing> ListForFolloweds(IEnumerable<string> ids);
    }
}