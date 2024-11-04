using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class NeedUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Skills { get; set; }

        [JsonPropertyName("interests")]
        public List<string>? Interests { get; set; }

        [JsonPropertyName("type")]
        public NeedType? Type { get; set; }
    }
}