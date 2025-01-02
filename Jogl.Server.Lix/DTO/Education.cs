using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Education
    {
        [JsonPropertyName("institutionName")]
        public string InstitutionName { get; set; }

        [JsonPropertyName("degree")]
        public string Degree { get; set; }

        [JsonPropertyName("fieldOfStudy")]
        public string FieldOfStudy { get; set; }

        [JsonPropertyName("dateStarted")]
        public string DateStarted { get; set; }

        [JsonPropertyName("dateEnded")]
        public string DateEnded { get; set; }

        [JsonPropertyName("timePeriod")]
        public TimePeriod TimePeriod { get; set; }
    }
}