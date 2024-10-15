using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface INodeService
    {
        Task<string> CreateAsync(Node node);
        Node Get(string nodeId, string userId);
        Node GetDetail(string nodeId, string userId);
        List<Node> Autocomplete(string userId, string search, int page, int pageSize);
        ListPage<Node> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long Count(string userId, string search);
        List<Node> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize);
        List<Node> ListForProject(string userId, string projectId, string search, int page, int pageSize);
        List<Node> ListForCommunity(string userId, string communityId, string search, int page, int pageSize);
        List<Node> ListForOrganization(string userId, string organizationId, string search, int page, int pageSize);
        Task UpdateAsync(Node node);
        Task DeleteAsync(string id);
    }
}