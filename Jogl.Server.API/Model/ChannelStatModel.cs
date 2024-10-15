using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ChannelStatModel
    {
        [JsonPropertyName("members_count")]
        public int MemberCount { get; set; }
    }
}