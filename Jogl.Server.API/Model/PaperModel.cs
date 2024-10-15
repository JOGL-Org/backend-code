using System.Text.Json.Serialization;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;

namespace Jogl.Server.API.Model
{
    public class PaperModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("authors")]
        public string Authors { get; set; }

        [JsonPropertyName("journal")]
        public string Journal { get; set; }

        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [JsonPropertyName("tags")]
        public List<PaperTag> Tags { get; set; }

        [JsonPropertyName("type")]
        public PaperType Type { get; set; }

        [JsonPropertyName("external_system")]
        public ExternalSystem ExternalSystem { get; set; }

        [JsonPropertyName("publication_date")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("user_in_library")]
        public bool UserInLibrary { get; set; }

        [JsonPropertyName("user_ids")]
        public List<string> UserIds { get; set; }

        [JsonPropertyName("open_access_pdf")]
        public string OpenAccessPdfUrl { get; set; }

        [JsonPropertyName("feed_count")]
        public int FeedCount { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel FeedStats { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }
    }
}