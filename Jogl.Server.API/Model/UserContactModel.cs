using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserContactModel
    {
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        [JsonPropertyName("EmailAddress")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [JsonPropertyName("Phone")]
        public string Phone { get; set; }

        [JsonPropertyName("Organization")]
        public string Organization { get; set; }

        [JsonPropertyName("OrganizationSize")]
        public string OrganizationSize { get; set; }

        [JsonPropertyName("Message")]
        public string Message { get; set; }

        [JsonPropertyName("Reason")]
        public string Reason { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }
    }
}