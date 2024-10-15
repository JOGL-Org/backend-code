using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ChannelMemberUpsertModel
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("access_level")]
        public SimpleAccessLevel AccessLevel { get; set; }
    }
}