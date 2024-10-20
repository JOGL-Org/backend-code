﻿using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IPaperService
    {
        Task<string> CreateAsync(string entityId, Paper paper);
        Paper Get(string paperId, string currentUserId);
        Paper GetDraft(string entityId, string userId);
        List<Paper> ListForExternalIds(IEnumerable<string> externalIds);
        List<Paper> ListForAuthor(string currentUserId, string userId, PaperType? type, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<Paper> ListForEntity(string currentUserId, string entityId, PaperType? type, List<PaperTag>? tags, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        ListPage<Paper> ListForCommunity(string currentUserId, string communityId, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag>? tags, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        ListPage<Paper> ListForNode(string currentUserId, string nodeId, List<CommunityEntityType> types, List<string> communityEntityIds, PaperType? type, List<PaperTag>? tags, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long CountForNode(string userId, string nodeId, string search);
        Task AssociateAsync(string entityId, string paperId, List<string> userIds = null);
        Task DisassociateAsync(string entityId, string paperId);
        Task UpdateAsync(Paper paper);
        [Obsolete]
        Task DeleteForExternalSystemAndUserAsync(string userId, ExternalSystem externalSystem);

        List<CommunityEntity> ListCommunityEntitiesForCommunityPapers(string currentUserId, string communityId, List<CommunityEntityType> types, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize);
        List<CommunityEntity> ListCommunityEntitiesForNodePapers(string currentUserId, string nodeId, List<CommunityEntityType> types, PaperType? type, List<PaperTag> tags, string search, int page, int pageSize);
    }
}