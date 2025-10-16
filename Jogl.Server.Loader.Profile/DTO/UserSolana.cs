using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class UserSolana
    {
        [JsonPropertyName("profile_colloseum")]
        public ColloseumProfile ColloseumProfile { get; set; }

        [JsonPropertyName("repos")]
        public List<Repo> Repos { get; set; } = new List<Repo>();
    }

    public class ColloseumProfile {

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}