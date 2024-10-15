using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProjectUpsertModel : CommunityEntityUpsertModel
    {
        [JsonPropertyName("maturity")]
        public string? Maturity { get; set; }

        //[JsonPropertyName("geoloc")]
        //public GeolocationModel Geolocation { get; set; }

        [JsonPropertyName("is_looking_for_collaborators")]
        public bool IsLookingForCollaborators { get; set; }
    }
}