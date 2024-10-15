using Jogl.Server.Data;
using Jogl.Server.DB;

namespace Jogl.Server.Business
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task CreateTagAsync(Tag tag)
        {
            await _tagRepository.CreateAsync(tag);
        }

        public List<Tag> GetTags(string search, int page, int pageSize)
        {
            return _tagRepository.List(s => string.IsNullOrEmpty(search) || s.Text.StartsWith(search) && !s.Deleted, page, pageSize);
        }

        public Tag GetTag(string text)
        {
            return _tagRepository.Get(s => s.Text == text && !s.Deleted);
        }
    }
}