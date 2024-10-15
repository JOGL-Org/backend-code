using System.Text.Json.Serialization;
using Jogl.Server.Data;

namespace Jogl.Server.API.Model
{
    public class NotificationModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public NotificationType Type { get; set; }

        [JsonPropertyName("actioned")]
        public bool Actioned { get; set; }

        [JsonPropertyName("data")]
        public List<NotificationDataModel> Data { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}