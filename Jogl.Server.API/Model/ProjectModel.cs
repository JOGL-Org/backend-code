using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProjectModel : CommunityEntityModel
    {
        [JsonPropertyName("maturity")]
        public string Maturity { get; set; }

        [JsonPropertyName("is_looking_for_collaborators")]
        public bool IsLookingForCollaborators { get; set; }

        [JsonPropertyName("needs_created_by_any_member")]
        public bool NeedsCreatedByAnyMember { get; set; }

        [JsonPropertyName("paper_created_by_any_member")]
        public bool PapersCreatedByAnyMember { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }
    }
}