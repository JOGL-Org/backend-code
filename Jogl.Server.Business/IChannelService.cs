using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IChannelService
    {
        Channel Get(string id, string userId);
        Channel GetDetail(string id, string userId);
        List<Channel> ListAgentChannels(string userId, SortKey sortKey, bool sortAscending);
        List<Channel> ListForEntity(string userId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        bool ListForNodeHasNewContent(string currentUserId, string nodeId);
        Task<string> CreateAsync(Channel channel);
        Task UpdateAsync(Channel channel);
        Task DeleteAsync(string id);
    }
}
