using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface ITagService
    {
        Task CreateTagAsync(Tag tag);
        List<Tag> GetTags(string search, int page, int pageSize);
        Tag GetTag(string text);
    }
}