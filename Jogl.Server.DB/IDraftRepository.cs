using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IDraftRepository : IRepository<Draft>
    {
        Task SetDraftAsync(string entityId, string userId, string text, DateTime updatedUTC);
    }
}