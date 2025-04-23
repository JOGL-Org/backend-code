using Jogl.Server.AI;

namespace Jogl.Server.AI.Agent
{
    public interface IAgent
    {
        Task<string> GetResponseAsync(IEnumerable<InputItem> messages, Dictionary<string, string> emailHandles, string? nodeId = default);
    }
}
