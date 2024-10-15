using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ChannelExtendedModel : ChannelModel
    {
        [JsonPropertyName("unread_posts")]
        public int UnreadPosts { get; set; }

        [JsonPropertyName("unread_mentions")]
        public int UnreadMentions { get; set; }

        [JsonPropertyName("unread_threads")]
        public int UnreadThreads { get; set; }

        [JsonIgnore]
        public override ChannelStatModel Stats { get => base.Stats; set => base.Stats = value; }
    }
}