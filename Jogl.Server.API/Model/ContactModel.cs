using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ContactModel
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
       
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}