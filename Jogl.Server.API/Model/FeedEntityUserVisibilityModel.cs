using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedEntityUserVisibilityModel
    {
        [JsonPropertyName("visibility")]
        public FeedEntityVisibility Visibility { get; set; }

        [JsonPropertyName("user")]
        public UserMiniModel User { get; set; }
    }
}