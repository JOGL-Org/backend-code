using System.Text.Json.Serialization;

namespace Jogl.Server.GitHub.DTO
{
    public class Link
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }
}