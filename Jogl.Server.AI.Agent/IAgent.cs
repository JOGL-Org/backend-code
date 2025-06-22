using Jogl.Server.AI.Agent.DTO;

namespace Jogl.Server.AI.Agent
{
    public interface IAgent
    {
        Task<AgentResponse> GetInitialResponseAsync(IEnumerable<InputItem> messages, Dictionary<string, string> emailHandles, string? nodeId = default, string? interfaceType = default);
        Task<AgentResponse> GetFollowupResponseAsync(IEnumerable<InputItem> messages, string context, string? interfaceType = default);
    }
}