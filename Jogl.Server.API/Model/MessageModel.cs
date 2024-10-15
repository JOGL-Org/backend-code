using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class MessageModel
    {
        [JsonPropertyName("user_ids")]
        public List<string> UserIds { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}