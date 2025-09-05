using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Skill
    {
        [JsonPropertyName("skill")]
        public string SkillName { get; set; }
    }
}
