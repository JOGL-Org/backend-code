using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface ISearchService
    {
        List<CommunityEntity> SearchCommunityEntities(string search);
    }
}
