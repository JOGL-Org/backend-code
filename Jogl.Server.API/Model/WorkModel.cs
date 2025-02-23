using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class WorkModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("authors")]
        public List<string> Authors { get; set; }

        [JsonPropertyName("publication")]
        public string Publication { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }
}