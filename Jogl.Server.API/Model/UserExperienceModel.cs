using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserExperienceModel
    {
        [JsonPropertyName("company")]
        public string Company { get; set; }
        [JsonPropertyName("position")]
        public string Position { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("date_from")]
        public string DateFrom { get; set; }
        [JsonPropertyName("date_to")]
        public string DateTo { get; set; }
    }
}