using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ErrorModel
    {
        public ErrorModel(string error)
        {
            Error = error;
        }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}