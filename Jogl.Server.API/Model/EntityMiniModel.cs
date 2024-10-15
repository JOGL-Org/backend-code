using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EntityMiniModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("full_name")]
        public string Fullname { get; set; }

        [JsonPropertyName("short_title")]
        public string ShortTitle { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("banner_url")]
        public string BannerUrl { get; set; }

        [JsonPropertyName("banner_url_sm")]
        public string BannerUrlSmall { get; set; }

        [JsonPropertyName("logo_url")]
        public string LogoUrl { get; set; }

        [JsonPropertyName("logo_url_sm")]
        public string LogoUrlSmall { get; set; }

        [JsonPropertyName("entity_type")]
        public FeedType EntityType { get; set; }
    }
}