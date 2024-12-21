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
        List<Need> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        bool ListForEntityHasNew(string currentUserId, string entityId);
        ListPage<Need> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        bool ListForNodeHasNew(string currentUserId, string entityId, FeedEntityFilter? filter);
        long CountForNode(string currentUserId, string nodeId, string search);
        Task<string> CreateAsync(Need need);
        Task UpdateAsync(Need need);
        Task DeleteAsync(string id);

        List<CommunityEntity> ListCommunityEntitiesForNodeNeeds(string currentUserId, string nodeId, string search, int page, int pageSize);
    }
}