using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class RepositoryModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}