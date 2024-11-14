using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IPaperService
    {
        Task<string> CreateAsync(Paper paper);
        Paper Get(string paperId, string currentUserId);
        Paper GetDraft(string entityId, string userId);
        List<Paper> ListForAuthor(string currentUserId, string userId, PaperType? type, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Paper> ListForEntity(string currentUserId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        ListPage<Paper> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long CountForNode(string userId, string nodeId, string search);
        Task AssociateAsync(string entityId, string paperId, List<string> userIds = null);
        Task DisassociateAsync(string entityId, string paperId);
        Task UpdateAsync(Paper paper);
        [Obsolete]
        Task DeleteForExternalSystemAndUserAsync(string userId, ExternalSystem externalSystem);
        Task DeleteAsync(Paper paper);

        List<CommunityEntity> ListCommunityEntitiesForNodePapers(string currentUserId, string nodeId, string search, int page, int pageSize);
    }
}