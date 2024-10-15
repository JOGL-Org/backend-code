using System.Text.Json.Serialization;

namespace Jogl.Server.LinkedIn.DTO
{
    public class UserInfo
    {
        [JsonPropertyName("sub")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string LastName { get; set; }

        [JsonPropertyName("picture")]
        public string profileUrl { get; set; }

        [JsonPropertyName("email")]
        public string email { get; set; }
    }
}
