using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public abstract class CommunityEntityModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("short_title")]
        public string ShortTitle { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("banner_url_sm")]
        public string BannerUrlSmall { get; set; }

        [JsonPropertyName("banner_url")]
        public string BannerUrl { get; set; }

        [JsonPropertyName("logo_url_sm")]
        public string LogoUrlSmall { get; set; }

        [JsonPropertyName("logo_url")]
        public string LogoUrl { get; set; }

        [JsonPropertyName("feed_id")]
        public string FeedId { get; set; }

        //[JsonPropertyName("geoloc")]
        //public GeolocationModel Geolocation { get; set; }

        [JsonPropertyName("interests")]
        public List<string> Interests { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; }

        [JsonPropertyName("links")]
        public List<LinkModel> Links { get; set; }

        [JsonPropertyName("tabs")]
        public List<string> Tabs { get; set; }

        [JsonPropertyName("listing_privacy")]
        public PrivacyLevel ListingPrivacy { get; set; }

        [JsonPropertyName("content_privacy")]
        public PrivacyLevel ContentPrivacy { get; set; }

        [JsonPropertyName("content_privacy_custom_settings")]
        public List<PrivacyLevelSettingModel> ContentPrivacyCustomSettings { get; set; }

        [JsonPropertyName("joining_restriction")]
        public JoiningRestrictionLevel JoiningRestrictionLevel { get; set; }

        [JsonPropertyName("joining_restriction_custom_settings")]
        public List<JoiningRestrictionLevelSettingModel> JoiningRestrictionLevelCustomSettings { get; set; }

        [JsonPropertyName("management")]
        public List<string> Settings { get; set; }

        [JsonPropertyName("home_channel_id")]
        public string? HomeChannelId { get; set; }

        [JsonPropertyName("user_access_level")]
        public string UserAccessLevel { get; set; }

        [JsonPropertyName("stats")]
        public virtual CommunityEntityStatModel Stats { get; set; }

        [JsonPropertyName("user_access")]
        public CommunityEntityPermissionModel Access { get; set; }

        [JsonPropertyName("user_onboarded")]
        public bool Onboarded { get; set; }

        [JsonPropertyName("contribution")]
        public string Contribution { get; set; }

        [JsonPropertyName("listing_origin")]
        public AccessOriginModel AccessOrigin { get; set; }

        [JsonPropertyName("path")]
        public List<EntityMiniModel> Path { get; set; }

        [JsonPropertyName("user_invitation")]
        public InvitationModelUser Invitation { get; set; }
    }
}