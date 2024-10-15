using System.Text.Json.Serialization;

namespace Jogl.Server.GitHub.DTO
{
    public class Commit
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("ref")]
        public string Ref { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }

        [JsonPropertyName("repo")]
        public Repo Repo { get; set; }
    }
}