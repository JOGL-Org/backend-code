using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class AuthResultExtendedModel : AuthResultModel
    {
        [JsonPropertyName("created")]
        public bool Created { get; set; }
    }
}