using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class WaitlistRecordModel
    {
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        [JsonPropertyName("EmailAddress")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [JsonPropertyName("Organization")]
        public string Organization { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }
    }
}