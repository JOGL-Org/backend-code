using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserLoginSignatureModel
    {
        [JsonPropertyName("wallet")]
        public string Wallet { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }
    }
}