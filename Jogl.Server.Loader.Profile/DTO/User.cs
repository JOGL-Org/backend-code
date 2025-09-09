using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class User
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("headline")]
        public string Headline { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("experience")]
        public List<Experience> Experience { get; set; } = new List<Experience>();

        [JsonPropertyName("education")]
        public List<Education> Education { get; set; } = new List<Education>();

        [JsonPropertyName("skills")]
        public List<Skill> Skills { get; set; } = new List<Skill>();

        [JsonPropertyName("latest_activities")]
        public string LatestActivities { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("patents")]
        public Patents Patents { get; set; }

        [JsonPropertyName("papers")]
        public List<Paper> Papers { get; set; } = new List<Paper>();
    }
}