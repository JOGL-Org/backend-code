using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IResourceService
    {
        Resource Get(string resourceId, string userId);
        ListPage<Resource> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long Count(string userId, string search);
        List<Resource> ListForUser(string userId, string targetUserId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Resource> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending, bool recordListings = true);
        bool ListForEntityHasNew(string currentUserId, string entityId);
        ListPage<Resource> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        bool ListForNodeHasNew(string currentUserId, string entityId, FeedEntityFilter? filter);
        long CountForNode(string currentUserId, string nodeId, string search);
        Task<string> CreateAsync(Resource resource);
        Task UpdateAsync(Resource resource);
        Task DeleteAsync(Resource resource);

        Task<Resource> BuildResourceForRepoAsync(string repoUrl, string? accessToken = default);
    }
}