using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class PublicationModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}