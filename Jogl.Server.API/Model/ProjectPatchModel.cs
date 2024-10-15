using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProjectPatchModel : CommunityEntityPatchModel
    {
        [JsonPropertyName("maturity")]
        public string? Maturity { get; set; }

        [JsonPropertyName("is_looking_for_collaborators")]
        public bool? IsLookingForCollaborators { get; set; }
    }
}