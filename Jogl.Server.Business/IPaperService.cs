using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IPaperService
    {
        Task<string> CreateAsync(Paper paper);
        Paper Get(string paperId, string currentUserId);
        List<Paper> ListForAuthor(string currentUserId, string userId, PaperType? type, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Paper> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        bool ListForEntityHasNew(string currentUserId, string entityId);
        ListPage<Paper> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        bool ListForNodeHasNew(string currentUserId, string nodeId, FeedEntityFilter? filter);
        long CountForNode(string currentUserId, string nodeId, string search);
        Task UpdateAsync(Paper paper);
        Task DeleteForExternalSystemAndUserAsync(string userId, ExternalSystem externalSystem);
        Task DeleteAsync(Paper paper);

        List<CommunityEntity> ListCommunityEntitiesForNodePapers(string currentUserId, string nodeId, string search, int page, int pageSize);
    }
}