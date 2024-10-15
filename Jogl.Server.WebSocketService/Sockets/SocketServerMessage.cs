using System.Text.Json.Serialization;

namespace Jogl.Server.WebSocketService.Sockets
{
    public enum ServerMessageType { Notification, Mention, PostInFeed, CommentInPost, FeedActivity }
    public class SocketServerMessage
    {
        [JsonPropertyName("type")]
        public ServerMessageType Type { get; set; }
        [JsonPropertyName("topic_id")]
        public string TopicId { get; set; }
        [JsonPropertyName("subject_id")]
        public string SubjectId { get; set; }
    }
}