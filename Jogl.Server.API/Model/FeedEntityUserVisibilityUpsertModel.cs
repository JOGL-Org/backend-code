using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class FeedEntityUserVisibilityUpsertModel
    {
        [JsonPropertyName("visibility")]
        public FeedEntityVisibility Visibility { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
    }
}