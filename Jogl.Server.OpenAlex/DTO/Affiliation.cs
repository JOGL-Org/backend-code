using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Affiliation
    {
        [JsonPropertyName("institution")]
        public Institution Institution { get; set; }

        [JsonPropertyName("years")]
        public List<int> Years { get; set; }
    }
}