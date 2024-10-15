using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class SearchResultNodeModel
    {
        [JsonPropertyName("user_count")]
        public long UserCount { get; set; }

        [JsonPropertyName("workspace_count")]
        public long WorkspaceCount { get; set; }

        [JsonPropertyName("event_count")]
        public long EventCount { get; set; }

        [JsonPropertyName("need_count")]
        public long NeedCount { get; set; }

        [JsonPropertyName("doc_count")]
        public long DocCount { get; set; }

        [JsonPropertyName("paper_count")]
        public long PaperCount { get; set; }
    }
}