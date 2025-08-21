
namespace Jogl.Server.Conversation.Data
{
    public class Message
    {
        public string ConversationSystem { get; set; }
        public string MessageId { get; set; }
        public string ConversationId { get; set; }
        public string ChannelId { get; set; }
        public string WorkspaceId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
    }
}
