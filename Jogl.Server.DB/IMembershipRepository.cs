using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IMembershipRepository : IRepository<Membership>
    {
        Membership Get(string entityId, string userId);

        List<Membership> ListForUsers(IEnumerable<string> ids);
        List<Membership> ListForCommunityEntities(IEnumerable<string> ids);
    }
}