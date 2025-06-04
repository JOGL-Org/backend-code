using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class UserExperience
    {
        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("position")]

        public string Position { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("dateFrom")]
        public string? DateFrom { get; set; }

        [JsonPropertyName("dateTo")]
        public string? DateTo { get; set; }

        [JsonPropertyName("current")]
        public bool Current { get; set; }
    }
}