using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IConversationService
    {
        Conversation Get(string conversationId, string currentUserId);
        Conversation GetForUsers(IEnumerable<string> userIds, string currentUserId);
        ListPage<Conversation> List(string userId, string search, int page, int pageSize, SortKey sortKey, bool ascending);
        long Count(string userId, string search);
        Task<string> CreateAsync(Conversation conversation);
        Task UpdateAsync(Conversation conversation);
        Task DeleteAsync(Conversation conversation);
    }
}