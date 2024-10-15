using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface ICallForProposalRepository : IRepository<CallForProposal>
    {
        List<CallForProposal> ListForCommunityIds(IEnumerable<string> communityIds);
        Task UpdateTemplateAsync(CallForProposal updatedEntity);
    }
}