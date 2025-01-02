using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Skill
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("numOfEndorsement")]
        public string NumOfEndorsement { get; set; }
    }
}