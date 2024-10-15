using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class SearchResultGlobalModel
    {
        [JsonPropertyName("user_count")]
        public long UserCount { get; set; }

        [JsonPropertyName("node_count")]
        public long NodeCount { get; set; }

        [JsonPropertyName("event_count")]
        public long EventCount { get; set; }

        [JsonPropertyName("need_count")]
        public long NeedCount { get; set; }

        [JsonPropertyName("org_count")]
        public long OrgCount { get; set; }
    }
}