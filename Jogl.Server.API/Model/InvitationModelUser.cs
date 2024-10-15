using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class InvitationModelUser : UserMiniModel
    {
        [JsonPropertyName("invitation_id")]
        public string InvitationId { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("invitation_type")]
        public InvitationType Type { get; set; }
    }
}