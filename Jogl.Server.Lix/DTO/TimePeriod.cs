using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class TimePeriod
    {
        [JsonPropertyName("startedOn")]
        public Date StartedOn { get; set; }

        [JsonPropertyName("endedOn")]
        public Date EndedOn { get; set; }
    }
}