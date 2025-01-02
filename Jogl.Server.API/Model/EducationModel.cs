using System.Text.Json.Serialization;

namespace Jogl.Server.LinkedIn.DTO
{
    public class EducationModel
    {
        [JsonPropertyName("organization")]
        public string Organization { get; set; }

        [JsonPropertyName("program")]
        public string? Program { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }
        
        [JsonPropertyName("to")]
        public string? To { get; set; }
    }
}
