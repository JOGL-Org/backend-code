using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ImageConversionUpsertModel
    {
        [JsonPropertyName("to_format")]
        public string FormatTo { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}