using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IDocumentService
    {
        Task<string> CreateAsync(Document document);
        [Obsolete]
        Task<Document> GetAsync(string documentId, string currentUserId, bool loadData = true);
        Document Get(string documentId, string currentUserId);
        Document Get(string documentId);
        Task<Document> GetDataAsync(string documentId, string currentUserId);
        Document GetDraft(string entityId, string currentUserId);
        List<Document> ListForEntity(string currentUserId, string entityId, string folderId, DocumentFilter? type, string search, int page, int pageSize);
        ListPage<Document> ListForChannel(string currentUserId, string entityId, DocumentFilter type, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        ListPage<Document> ListForNode(string currentUserId, string nodeId, List<string> communityEntityIds, DocumentFilter? type, FeedEntityFilter? filter, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long CountForNode(string userId, string nodeId, string search);
        List<Document> ListAllDocuments(string currentUserId, string entityId, string search, int page, int pageSize);
        List<Document> ListAllDocuments(string currentUserId, string entityId);
        Task UpdateAsync(Document document);
        Task DeleteAsync(string id);

        Task<string> CreateFolderAsync(Folder folder);
        Folder GetFolder(string folderId);
        List<Folder> ListFolders(string entityId, string parentFolderId, string search, int page, int pageSize);
        List<Folder> ListAllFolders(string entityId, string search, int page, int pageSize);
        List<Folder> ListAllFolders(string entityId);
        Task UpdateFolderAsync(Folder folder);
        Task DeleteFolderAsync(string id);

        List<FeedEntity> ListFeedEntitiesForNodeDocuments(string currentUserId, string nodeId, List<CommunityEntityType> types, DocumentFilter? type, string search, int page, int pageSize);

        List<Entity> ListPortfolioForUser(string currentUserId, string userId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);

    }
}