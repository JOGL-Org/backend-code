using Jogl.Server.AI.Agent.DTO;

namespace Jogl.Server.AI.Agent
{
    public interface IAgent
    {
        Task<AgentResponse> GetInitialResponseAsync(string message, string? nodeId = default, string? interfaceType = default);
        Task<AgentResponse> GetFollowupResponseAsync(IEnumerable<InputItem> messages, string context, string originalQuery, string? interfaceType = default);
        Task<AgentConversationResponse> GetOnboardingResponseAsync(IEnumerable<InputItem> messages);
        Task<AgentResponse> GetFirstSearchResponseAsync(string current);
    }
}