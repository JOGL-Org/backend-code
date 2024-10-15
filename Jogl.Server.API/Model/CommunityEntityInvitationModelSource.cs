using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommunityEntityInvitationModelSource : CommunityEntityMiniModel
    {
        [JsonPropertyName("invitation_id")]
        public string InvitationId { get; set; }
    }
}