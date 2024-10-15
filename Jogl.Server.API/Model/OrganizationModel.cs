using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OrganizationModel : CommunityEntityModel
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }
    }
}