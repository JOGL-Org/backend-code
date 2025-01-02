using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Experience
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("dateStarted")]
        public string DateStarted { get; set; }

        [JsonPropertyName("dateEnded")]
        public string DateEnded { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("organisation")]
        public Organisation Organisation { get; set; }

        [JsonPropertyName("timePeriod")]
        public TimePeriod TimePeriod { get; set; }
    }
}