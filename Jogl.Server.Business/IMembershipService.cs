using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IMembershipService
    {
        Task<string> CreateAsync(Membership membership);
        Membership Get(string entityId, string userId);
        List<Membership> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        List<Membership> ListForEntities(string currentUserId, List<string> entityIds);

        int CountForEntity(string currentUserId, string entityId, string search);
        Task UpdateAsync(Membership membership);
        Task DeleteAsync(string id);
        Task DeleteAsync(Membership membership);

        Task AddMembersAsync(List<Membership> memberships, bool allowAddingOwners = false);
        Task UpdateMembersAsync(List<Membership> memberships, bool allowAddingOwners = false);
        Task SetMembersAsync(List<Membership> memberships, string communityEntityId, bool allowAddingOwners = false);
        List<Membership> ListMembers(string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        List<Membership> ListMembers(string entityId);
        Task RemoveMembersAsync(List<Membership> memberships);

        OnboardingQuestionnaireInstance GetOnboardingInstance(string entityId, string userId);
        Task<string> UpsertOnboardingInstanceAsync(OnboardingQuestionnaireInstance instance);
    }
}