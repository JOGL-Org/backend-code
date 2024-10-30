using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserLoginChallengeModel
    {
        [JsonPropertyName("wallet")]
        public string Wallet { get; set; }
    }
}