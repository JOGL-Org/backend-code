using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class PaperAuthorModelS2
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("papers")]
        public List<PaperModelS2> Papers { get; set; }
    }
}