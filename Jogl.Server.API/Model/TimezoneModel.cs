using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class TimezoneModel
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }
    }
}