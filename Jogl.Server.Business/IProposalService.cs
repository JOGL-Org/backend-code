using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IProposalService
    {
        Task<string> CreateAsync(Proposal proposal);
        Proposal Get(string currentUserId, string proposalId);
        Proposal GetForProjectAndCFP(string projectId, string callForProposalId);
        List<Proposal> ListForProject(string projectId);
        List<Proposal> ListForUser(string userId);
        List<Proposal> ListForCFPAdmin(string currentUserId, string callForProposalsId);
        List<Proposal> ListForCFP( string currentUserId,string callForProposalsId);
        Task UpdateAsync(Proposal proposal);
        Task JoinMembersToCFPAsync(Proposal proposal);
      
        Task DeleteAsync(string id);
    }
}