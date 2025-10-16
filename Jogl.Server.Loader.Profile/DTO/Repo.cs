using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Repo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("readme")]
        public string Readme { get; set; }

        [JsonPropertyName("homepage")]
        public string Homepage { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("topics")]
        public List<string> Topics { get; set; }

        [JsonPropertyName("readme_excerpt")]
        public string Abstract { get; set; }

        public string Url { get => $"https://github.com/{FullName}"; }
    }
}
