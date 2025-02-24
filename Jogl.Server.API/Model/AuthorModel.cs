using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AuthorModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("orcid_id")]
        public string OrcidId { get; set; }

        [JsonPropertyName("institutions")]
        public List<string> Institutions { get; set; }

        [JsonPropertyName("topics")]
        public List<string> Topics { get; set; }
    }
}