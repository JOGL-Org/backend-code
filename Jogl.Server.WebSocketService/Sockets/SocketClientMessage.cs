using System.Text.Json.Serialization;

namespace Jogl.Server.WebSocketService.Sockets
{
    public enum ClientMessageType { Subscribe, Unsubscribe }
    public class SocketClientMessage
    {
        [JsonPropertyName("type")]
        public ClientMessageType Type { get; set; }

        [JsonPropertyName("topic_id")]
        public string TopicId { get; set; }
    }
}