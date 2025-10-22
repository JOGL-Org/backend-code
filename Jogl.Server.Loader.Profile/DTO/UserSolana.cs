using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class UserSolana
    {
        [JsonPropertyName("profile_colloseum")]
        public ColloseumProfile ColloseumProfile { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("headline")]
        public string Headline { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("experience")]
        public List<Experience> Experience { get; set; } = new List<Experience>();

        [JsonPropertyName("education")]
        public List<Education> Education { get; set; } = new List<Education>();

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; }

        [JsonPropertyName("repos")]
        public List<Repo> Repos { get; set; }

        [JsonPropertyName("recent_github_activity")]
        public string RecentGithubActivity { get; set; }

        [JsonPropertyName("projects")]
        public List<Project> Projects { get; set; }

        [JsonPropertyName("lookingForCollab")]
        public bool LookingForCollab { get; set; }

        [JsonPropertyName("socialLinks")]
        public SocialLinks SocialLinks { get; set; }
    }

    public class ColloseumProfile
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}