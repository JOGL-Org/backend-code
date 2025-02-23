using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingUpsertModel
    {
        [JsonPropertyName("paperIds")]
        public List<string> PaperIds { get; set; }

        [JsonPropertyName("repos")]
        public List<string> Repos { get; set; }

        [JsonPropertyName("experience")]
        public List<UserExperienceModel>? Experience { get; set; }

        [JsonPropertyName("education")]
        public List<UserEducation>? Education { get; set; }
    }
}