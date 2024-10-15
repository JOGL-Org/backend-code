using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class MemberModel : UserMiniModel
    {
        [JsonPropertyName("access_level")]
        public AccessLevel AccessLevel { get; set; }

        [JsonPropertyName("membership_id")]
        public string MembershipId { get; set; }

        [JsonPropertyName("contribution")]
        public string Contribution { get; set; }

        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; }
    }
}