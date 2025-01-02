using Jogl.Server.LinkedIn.DTO;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProfileModel
    {
        [JsonPropertyName("education")]
        public List<EducationModel> Education { get; set; }

        [JsonPropertyName("experience")]
        public List<ExperienceModel> Experience { get; set; }
    }
}