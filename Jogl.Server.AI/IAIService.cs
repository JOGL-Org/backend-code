using Jogl.Server.Data;

namespace Jogl.Server.AI
{
    public interface IAIService
    {
        Task<decimal> GetBotScoreAsync<T>(T payload) where T : Entity;
    }
}
