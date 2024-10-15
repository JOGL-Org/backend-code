using Jogl.Server.Data;
using Jogl.Server.Data.Util;

namespace Jogl.Server.Business
{
    public interface IChannelService
    {
        Channel Get(string id, string userId);
        List<Channel> ListForEntity(string userId, string entityId, string search, int page, int pageSize, SortKey sortKey, bool sortAscending);
        Task<string> CreateAsync(Channel channel);
        Task UpdateAsync(Channel channel);
        Task DeleteAsync(string id);
    }
}
