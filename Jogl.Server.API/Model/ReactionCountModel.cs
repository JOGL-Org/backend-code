using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ReactionCountModel
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}