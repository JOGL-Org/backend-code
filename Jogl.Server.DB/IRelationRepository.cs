using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IRelationRepository : IRepository<Relation>
    {
        List<Relation> ListForSourceOrTargetIds(IEnumerable<string> ids);
        List<Relation> ListForSourceIds(IEnumerable<string> ids);
        List<Relation> ListForTargetIds(IEnumerable<string> ids);
        List<Relation> ListForSourceIds(IEnumerable<string> ids, CommunityEntityType targetEntityType);
        List<Relation> ListForTargetIds(IEnumerable<string> ids, CommunityEntityType sourceEntityType);
    }
}