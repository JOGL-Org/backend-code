using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface ICallForProposalService
    {
        CallForProposal Get(string callForProposalId, string userId);
        CallForProposal GetDetail(string callForProposalId, string userId);
        List<CallForProposal> Autocomplete(string userId, string search, int page, int pageSize);
        ListPage<CallForProposal> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        List<CallForProposal> ListForCommunity(string userId, string communityId, string search, int page, int pageSize);
        List<CallForProposal> ListForNode(string userId, string nodeId, string search, int page, int pageSize);
        int CountForNode(string userId, string nodeId, string search);
        bool HasTemplateChanged(CallForProposal existingCFP, CallForProposal updatedCFP);
        Task<string> CreateAsync(CallForProposal callForProposal);
        Task UpdateAsync(CallForProposal callForProposal);
        Task DeleteAsync(string id);

        Task SendMessageToUsersAsync(string cfpId, List<string> userIds, string subject, string message, string url);
    }
}