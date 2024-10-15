using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class DiscussionResponse
    {
        [JsonPropertyName("discussions")]
        public List<Discussion> Discussions { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("numClosedDiscussions")]
        public int NumClosedDiscussions { get; set; }
    }


}