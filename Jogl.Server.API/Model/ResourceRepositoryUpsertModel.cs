using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ResourceRepositoryUpsertModel
    {
        [JsonPropertyName("repo_url")]
        public string RepositoryUrl { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}