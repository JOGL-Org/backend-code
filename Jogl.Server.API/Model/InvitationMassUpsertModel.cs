using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class InvitationMassUpsertModel
    {
        [JsonPropertyName("invitees")]
        public required List<InvitationUpsertModel> Invitees { get; set; }

        [JsonPropertyName("community_entity_ids")]
        public required List<string> CommunityEntityIds { get; set; }
    }
}