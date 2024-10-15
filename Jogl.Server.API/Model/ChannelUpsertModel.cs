using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ChannelUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

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
        public List<string>? Settings { get; set; }

        [JsonPropertyName("members")]
        public List<ChannelMemberUpsertModel>? Members { get; set; }
    }
}