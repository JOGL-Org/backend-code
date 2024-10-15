//using Jogl.Server.Data;
//using Jogl.Server.Data.Enum;
//using Jogl.Server.Data.Util;

//namespace Jogl.Server.Business
//{
//    public interface IProjectService
//    {
//        Project Get(string projectId, string userId);
//        Project GetDetail(string projectId, string userId);
//        List<Project> Autocomplete(string userId, string search, int page, int pageSize);
//        ListPage<Project> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
//        List<Project> ListForUser(string userId, string targetUserId, Permission? permission, string search, int page, int pageSize);
//        List<Project> ListForPaperExternalId(string userId, string externalId);
//        List<Project> ListForCommunity(string userId, string communityId, string search, int page, int pageSize);
//        List<Project> ListForNode(string userId, string nodeId, string search, int page, int pageSize);
//        List<Project> ListForOrganization(string userId, string organizationId, string search, int page, int pageSize);
//        int CountForNode(string userId, string nodeId, string search);
//        Task<string> CreateAsync(Project project);
//        Task UpdateAsync(Project project);
//        Task DeleteAsync(string id);
//    }
//}