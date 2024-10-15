using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class Repo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}