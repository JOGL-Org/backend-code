using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class TopicOuter
    {
        [JsonPropertyName("topic")]
        public TopicInner Topic { get; set; } = new TopicInner { };
    }

    public class TopicInner
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
