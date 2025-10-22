using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class SocialLinks
    {
        [JsonPropertyName("githubHandle")]
        public string GithubHandle { get; set; }

        [JsonPropertyName("linkedinHandle")]
        public string LinkedinHandle { get; set; }

        [JsonPropertyName("twitterHandle")]
        public string TwitterHandle { get; set; }

        [JsonPropertyName("telegramHandle")]
        public string TelegramHandle { get; set; }

        [JsonPropertyName("colloseumHandle")]
        public string ColloseumHandle { get; set; }
    }
}
