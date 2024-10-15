using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IWorkspaceService
    {
        Workspace Get(string communityId, string userId);
        Workspace GetDetail(string communityId, string userId);
        List<Workspace> Autocomplete(string userId, string search, int page, int pageSize);
        ListPage<Workspace> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Workspace> ListForUser(string userId, string ecosystemUserId, Permission? permission, string search, int page, int pageSize);
        List<Workspace> ListForWorkspace(string userId, string workspaceId, string search, int page, int pageSize);
        List<Workspace> ListForNode(string userId, string nodeId, string search, int page, int pageSize);
        long CountForNode(string userId, string nodeId, string search);
        List<Workspace> ListForPaperExternalId(string userId, string externalId);
        List<Workspace> ListForOrganization(string userId, string organizationId, string search, int page, int pageSize);
        Task<string> CreateAsync(Workspace community);
        Task UpdateAsync(Workspace community);
        Task DeleteAsync(string id);
    }
}