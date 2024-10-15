using System.Text.Json.Serialization;
using Jogl.Server.Orcid.DTO;

namespace Jogl.Server.API.Model
{
    public class OrcidExperienceModel
    {
        [JsonPropertyName("educations")]
        public List<Education> EducationItems { get; set; }

        [JsonPropertyName("employments")]
        public List<Employment> EmploymentItems { get; set; }
    }
}