using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommunityEntityChannelModel : CommunityEntityMiniModel
    {
        [JsonPropertyName("channels")]
        public List<ChannelExtendedModel> Channels { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }
    }
}