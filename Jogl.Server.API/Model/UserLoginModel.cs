using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserLoginModel
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}