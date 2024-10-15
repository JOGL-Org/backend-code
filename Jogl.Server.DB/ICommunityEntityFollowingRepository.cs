using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface ICommunityEntityFollowingRepository : IRepository<CommunityEntityFollowing>
    {
        CommunityEntityFollowing GetFollowing(string userIdFrom, string userIdTo);
        List<CommunityEntityFollowing> ListForFollowers(IEnumerable<string> ids);
        List<CommunityEntityFollowing> ListForFolloweds(IEnumerable<string> ids);
    }
}