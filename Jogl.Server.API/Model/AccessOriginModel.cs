using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AccessOriginModel
    {
        [JsonPropertyName("type")]
        public AccessOriginType Type { get; set; }

        [JsonPropertyName("ecosystem_memberships")]
        public List<MembershipModel> EcosystemMemberships { get; set; }
    }
}