using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Date
    {
        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}