using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class TextValueModel
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}