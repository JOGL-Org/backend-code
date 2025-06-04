using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ConversationModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("user")]
        public UserMiniModel User { get; set; }

        [JsonPropertyName("latest_message")]
        public ContentEntityModel LatestMessage { get; set; }
    }
}