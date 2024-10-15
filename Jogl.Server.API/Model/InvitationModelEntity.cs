using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class InvitationModelEntity : CommunityEntityMiniModel
    {
        [JsonPropertyName("invitation_id")]
        public string InvitationId { get; set; }

        [JsonPropertyName("entity_type")]
        public CommunityEntityType EntityType { get; set; }

        [JsonPropertyName("invitation_type")]
        public InvitationType Type { get; set; }
    }
}