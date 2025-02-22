using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AuthorModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}