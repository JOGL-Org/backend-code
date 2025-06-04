using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class UserEducation
    {
        [JsonPropertyName("school")]
        public string School { get; set; }

        [JsonPropertyName("program")]
        public string? Program { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("dateFrom")]
        public string? DateFrom { get; set; }

        [JsonPropertyName("dateTo")]
        public string? DateTo { get; set; }

        [JsonPropertyName("currentt")]
        public bool Current { get; set; }
    }
}