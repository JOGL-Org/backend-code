namespace Jogl.Server.AI.Agent.DTO
{
    public class AgentResponse
    {
        public AgentResponse()
        {
            Text = new List<string>();
        }

        public AgentResponse(string text)
        {
            Text = [text];
        }

        public List<string> Text { get; set; }
        public string Context { get; set; }
        public string OriginalQuery { get; set; }
    }
}
