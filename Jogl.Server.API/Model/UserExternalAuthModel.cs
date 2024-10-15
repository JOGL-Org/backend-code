using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserExternalAuthModel
    {
        [JsonPropertyName("orcid_access_token")]
        public string OrcidAccessToken { get; set; }

        [JsonPropertyName("is_orcid_user")]
        public bool IsOrcidUser { get; set; }

        [JsonPropertyName("is_google_user")]
        public bool IsGoogleUser { get; set; }
    }
}