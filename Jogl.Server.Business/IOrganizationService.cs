using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IOrganizationService
    {
        Organization Get(string communityId, string userId);
        Organization GetDetail(string communityId, string userId);
        List<Organization> Autocomplete(string userId, string search, int page, int pageSize);
        ListPage<Organization> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long Count(string userId, string search);
        List<Organization> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize);
        List<Organization> ListForProject(string userId, string projectId, string search, int page, int pageSize);
        List<Organization> ListForCommunity(string userId, string communityId, string search, int page, int pageSize);
        List<Organization> ListForNode(string userId, string nodeId, string search, int page, int pageSize);
        int CountForNode(string userId, string nodeId, string search);
        Task<string> CreateAsync(Organization organization);
        Task UpdateAsync(Organization organization);
        Task DeleteAsync(string id);
    }
}