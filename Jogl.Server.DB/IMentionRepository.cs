using Jogl.Server.Data;

namespace Jogl.Server.DB
{
    public interface IMentionRepository : IRepository<Mention>
    {
        Task SetMentionReadAsync(Mention mention);
    }
}