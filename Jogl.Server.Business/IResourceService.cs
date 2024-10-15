using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IResourceService
    {
        Resource Get(string resourceId);
        List<Resource> ListForFeed(string feedId, string search, int page, int pageSize);
        List<Resource> ListForNode(string currentUserId, string nodeId, string search, int page, int pageSize);
        int CountForNode(string currentUserId, string nodeId, PaperType? type, string search);
        Task<string> CreateAsync(Resource resource);
        Task UpdateAsync(Resource resource);
        Task DeleteAsync(string id);
    }
}