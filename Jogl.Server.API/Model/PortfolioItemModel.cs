using Jogl.Server.Data.Enum;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public enum PortfolioItemType { Paper, JoglDoc, Repository }
    public class PortfolioItemModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("authors")]
        public string Authors { get; set; }

        [JsonPropertyName("type")]
        public PortfolioItemType Type { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel FeedStats { get; set; }

        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; set; }

        [JsonPropertyName("publication_date")]
        public string PublicationDate { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [JsonPropertyName("journal")]
        public string Journal { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}