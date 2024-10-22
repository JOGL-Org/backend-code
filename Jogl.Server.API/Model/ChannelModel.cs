using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ChannelModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("icon_key")]
        public string IconKey { get; set; }

        [JsonPropertyName("visibility")]
        public ChannelVisibility Visibility { get; set; }

        [JsonPropertyName("auto_join")]
        public bool AutoJoin { get; set; }

        [JsonPropertyName("management")]
        public List<string> Settings { get; set; }

        [JsonPropertyName("user_access_level")]
        public AccessLevel? CurrentUserAccessLevel { get; set; }

        [JsonPropertyName("stats")]
        public virtual ChannelStatModel Stats { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }
    }
}