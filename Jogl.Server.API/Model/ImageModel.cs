using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ImageModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("file_name")]
        public string Filename { get; set; }

        [JsonPropertyName("file_type")]
        public string Filetype { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}