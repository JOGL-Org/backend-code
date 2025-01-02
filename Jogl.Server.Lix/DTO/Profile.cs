using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Profile
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("img")]
        public string Img { get; set; }

        [JsonPropertyName("aboutSummaryText")]
        public string AboutSummaryText { get; set; }

        [JsonPropertyName("salesNavLink")]
        public string SalesNavLink { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("experience")]
        public List<Experience> Experience { get; set; }

        [JsonPropertyName("education")]
        public List<Education> Education { get; set; }

        [JsonPropertyName("skills")]
        public List<Skill> Skills { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("numOfConnections")]
        public string NumOfConnections { get; set; }

        [JsonPropertyName("pronoun")]
        public string Pronoun { get; set; }

        [JsonPropertyName("languages")]
        public List<Language> Languages { get; set; }
    }
}