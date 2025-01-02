using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Language
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("proficiency")]
        public string Proficiency { get; set; }
    }
}