using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ImageInsertModel : BaseModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("file_name")]
        public string Filename { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}