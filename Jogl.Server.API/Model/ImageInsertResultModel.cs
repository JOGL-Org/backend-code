using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ImageInsertResultModel 
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}