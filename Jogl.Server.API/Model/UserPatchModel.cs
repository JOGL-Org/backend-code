using Jogl.Server.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserPatchModel
    {
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress]
        public string? Email { get; set; }

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("banner_id")]
        public string? BannerId { get; set; }

        [JsonPropertyName("logo_id")]
        public string? AvatarId { get; set; }

        [JsonPropertyName("short_bio")]
        public string? ShortBio { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("links")]
        public List<Link>? Links { get; set; }

        [JsonPropertyName("skills")]
        public List<string>? Skills { get; set; }

        [JsonPropertyName("interests")]
        public List<string>? Interests { get; set; }

        [JsonPropertyName("assets")]
        public List<string>? Assets { get; set; }

        [JsonPropertyName("orcid_id")]
        public string? OrcidId { get; set; }

        [JsonPropertyName("status")]
        public string? StatusText { get; set; }

        [JsonPropertyName("newsletter")]
        public bool? Newsletter { get; set; }

        [JsonPropertyName("contact_me")]
        public bool? ContactMe { get; set; }

        [JsonPropertyName("notification_settings")]
        public UserNotificationSettingsModel? NotificationSettings { get; set; }

        [JsonPropertyName("experience")]
        public List<UserExperienceModel>? Experience { get; set; }

        [JsonPropertyName("education")]
        public List<UserEducation>? Education { get; set; }

        [JsonPropertyName("external_auth")]
        public UserExternalAuthModel? Auth { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }
}