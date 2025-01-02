using System.Text.Json.Serialization;

namespace Jogl.Server.Lix.DTO
{
    public class Organisation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("salesNavLink")]
        public string SalesNavLink { get; set; }
    }
}