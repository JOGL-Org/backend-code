using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ContentEntityOverridesModel
    {
        [JsonPropertyName("user_image_url")]
        public string UserAvatarURL { get; set; }

        [JsonPropertyName("user_url")]
        public string UserURL { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
    }
}