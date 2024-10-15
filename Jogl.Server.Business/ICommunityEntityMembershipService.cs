using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface ICommunityEntityMembershipService
    {
        Task<string> CreateAsync(Relation relation);
        Relation GetForSourceAndTarget(string sourceEntityId, string targetEntityId);
        Task DeleteAsync(string id);
    }
}