using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OrganizationUpsertModel : CommunityEntityUpsertModel
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}