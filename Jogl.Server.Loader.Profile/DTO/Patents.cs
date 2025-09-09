using System.Text.Json.Serialization;

namespace Jogl.Server.Loader.Profile.DTO
{
    public class Patents
    {
        [JsonPropertyName("families")]
        public required List<Patent> PatentFamilies { get; set; }
    }
}
