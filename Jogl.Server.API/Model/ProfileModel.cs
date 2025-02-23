using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProfileModel
    {
        [JsonPropertyName("education")]
        public List<UserEducationModel> Education { get; set; }

        [JsonPropertyName("experience")]
        public List<UserExperienceModel> Experience { get; set; }
    }
}