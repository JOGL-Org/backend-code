using System.Text.Json.Serialization;

namespace Jogl.Server.InfoBIP.DTO
{
    public class ConversationMessageResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
