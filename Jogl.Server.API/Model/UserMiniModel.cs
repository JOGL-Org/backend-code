using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserMiniModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("short_bio")]
        public string ShortBio { get; set; }

        [JsonPropertyName("banner_url")]
        public string BannerUrl { get; set; }

        [JsonPropertyName("banner_url_sm")]
        public string BannerUrlSmall { get; set; }

        [JsonPropertyName("logo_url")]
        public string LogoUrl { get; set; }

        [JsonPropertyName("logo_url_sm")]
        public string LogoUrlSmall { get; set; }

        [JsonPropertyName("status")]
        public string StatusText { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("follower_count")]
        public int FollowerCount { get; set; }

        [JsonPropertyName("user_follows")]
        public bool UserFollows { get; set; }

        [JsonPropertyName("stats")]
        public CommunityEntityStatModel Stats { get; set; }

        [JsonPropertyName("experience")]
        public List<UserExperienceModel> Experience { get; set; }

        [JsonPropertyName("education")]
        public List<UserEducationModel> Education { get; set; }

        [JsonPropertyName("spaces")]
        public List<CommunityEntityMiniModel> Spaces { get; set; }
    }
}