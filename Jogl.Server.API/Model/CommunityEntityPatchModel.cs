using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public abstract class CommunityEntityPatchModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("short_title")]
        public string? ShortTitle { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("short_description")]
        public string? ShortDescription { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("banner_id")]
        public string? BannerId { get; set; }

        [JsonPropertyName("logo_id")]
        public string? LogoId { get; set; }

        [JsonPropertyName("interests")]
        public List<string>? Interests { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("links")]
        public List<LinkModel>? Links { get; set; }

        [JsonPropertyName("tabs")]
        public List<string>? Tabs { get; set; }

        [JsonPropertyName("listing_privacy")]
        public PrivacyLevel? ListingPrivacy { get; set; }

        [JsonPropertyName("content_privacy")]
        public PrivacyLevel? ContentPrivacy { get; set; }

        [JsonPropertyName("content_privacy_custom_settings")]
        public List<PrivacyLevelSettingModel>? ContentPrivacyCustomSettings { get; set; }

        [JsonPropertyName("joining_restriction")]
        public JoiningRestrictionLevel? JoiningRestrictionLevel { get; set; }

        [JsonPropertyName("joining_restriction_custom_settings")]
        public List<JoiningRestrictionLevelSettingModel>? JoiningRestrictionLevelCustomSettings { get; set; }
        
        [JsonPropertyName("management")]
        public List<string>? Settings { get; set; }
    }
}