using System.Text.Json.Serialization;

namespace Jogl.Server.GitHub.DTO
{
    public class UserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        //[JsonPropertyName("family_name")]
        //public string LastName { get; set; }

        //[JsonPropertyName("picture")]
        //public string profileUrl { get; set; }

        //[JsonPropertyName("email")]
        //public string email { get; set; }
    }
}
