using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class Sibling
    {
        [JsonPropertyName("rfilename")]
        public string Rfilename { get; set; }
    }
}