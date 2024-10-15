using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class EmailModel
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}