using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class BinaryDataModel
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}