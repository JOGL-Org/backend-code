using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IProposalRepository : IRepository<Proposal>
    {
        List<Proposal> ListForCFPIds(IEnumerable<string> ids);
    }
}