using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface INeedService
    {
        Need Get(string needId, string userId);
        ListPage<Need> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long Count(string userId, string search);
        List<Need> ListForUser(string userId, string targetUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Need> ListForEntity(string userId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        ListPage<Need> ListForCommunity(string userId, string communityId, List<string> communityEntityIds, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        ListPage<Need> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, bool currentUser, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long CountForNode(string currentUserId, string nodeId, List<string> communityEntityIds, string search);
        Task<string> CreateAsync(Need need);
        Task UpdateAsync(Need need);
        Task DeleteAsync(string id);

        List<CommunityEntity> ListCommunityEntitiesForNodeNeeds(string nodeId, string currentUserId, List<CommunityEntityType> types, bool currentUser, string search, int page, int pageSize);
        List<CommunityEntity> ListCommunityEntitiesForCommunityNeeds(string communityId, string currentUserId, List<CommunityEntityType> types, bool currentUser, string search, int page, int pageSize);
    }
}