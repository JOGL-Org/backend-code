using System.Text.Json.Serialization;
using Jogl.Server.Data;

namespace Jogl.Server.API.Model
{
    public class NotificationDataModel
    {
        [JsonPropertyName("key")]
        public NotificationDataKey Key { get; set; }
        
        [JsonPropertyName("entity_id")]
        public string EntityId { get; set; }

        [JsonPropertyName("community_entity_type")]
        public CommunityEntityType? CommunityEntityType { get; set; }
      
        [JsonPropertyName("community_entity_onboarding")]
        public OnboardingConfigurationModel? CommunityEntityOnboarding { get; set; }

        [JsonPropertyName("content_entity_type")]
        public ContentEntityType? ContentEntityType { get; set; }

        [JsonPropertyName("entity_title")]
        public string EntityTitle { get; set; }

        [JsonPropertyName("entity_subtype")]
        public string EntitySubtype { get; set; }

        [JsonPropertyName("entity_logo_url")]
        public string? EntityLogoUrl { get; set; }

        [JsonPropertyName("entity_logo_url_sm")]
        public string? EntityLogoUrlSmall { get; set; }

        [JsonPropertyName("entity_banner_url")]
        public string? EntityBannerUrl { get; set; }

        [JsonPropertyName("entity_banner_url_sm")]
        public string? EntityBannerUrlSmall { get; set; }

        [JsonPropertyName("entity_onboarding_enabled")]
        public bool EntityOnboardingAnswersAvailable { get; set; }

        [JsonPropertyName("entity_home_channel_id")]
        public string? EntityHomeChannelId { get; set; }
    }
}