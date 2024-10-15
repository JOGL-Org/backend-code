using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserPasswordChangeModel
    {
        [JsonPropertyName("old_password")]
        public string OldPassword { get; set; }

        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
    }
}