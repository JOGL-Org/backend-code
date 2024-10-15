using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EmailMessageModel
    {
        [JsonPropertyName("from_name")]
        public string FromName { get; set; }

        [JsonPropertyName("to_email")]
        public string ToEmail { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}