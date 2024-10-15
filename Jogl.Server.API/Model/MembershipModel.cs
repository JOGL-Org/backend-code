using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class MembershipModel : BaseModel
    {
        [JsonPropertyName("access_level")]
        public AccessLevel AccessLevel { get; set; }

        [JsonPropertyName("community_entity_id")]
        public string CommunityEntityId { get; set; }

        [JsonPropertyName("contribution")]
        public string Contribution { get; set; }

        [JsonPropertyName("community_entity_type")]
        public CommunityEntityType CommunityEntityType { get; set; }
    }
}