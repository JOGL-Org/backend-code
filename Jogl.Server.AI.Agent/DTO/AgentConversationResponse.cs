namespace Jogl.Server.AI.Agent.DTO
{
    public class AgentConversationResponse : AgentResponse
    {
        public AgentConversationResponse(string text, bool stop, string output = null) : base(text)
        {
            Stop = stop;
            Output = output;
        }

        public bool Stop { get; set; }
        public string Output { get; set; }
    }
}
