using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class InvitationUpsertModel
    {
        [JsonPropertyName("user_email")]
        public string? Email { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("access_level")]
        public AccessLevel AccessLevel { get; set; }
    }
}