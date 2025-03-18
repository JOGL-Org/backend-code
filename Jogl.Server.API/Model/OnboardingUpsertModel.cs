using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingUpsertModel
    {
        [JsonPropertyName("paperIds")]
        public List<string> PaperIds { get; set; }

        [JsonPropertyName("repos")]
        public List<string> Repos { get; set; }

        [JsonPropertyName("github_access_token")]
        public string? GithubAccessToken { get; set; }

        [JsonPropertyName("huggingface_access_token")]
        public string? HuggingfaceAccessToken { get; set; }

        [JsonPropertyName("experience")]
        public List<UserExperienceModel>? Experience { get; set; }

        [JsonPropertyName("education")]
        public List<UserEducationModel>? Education { get; set; }
    }
}