using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    [JsonDerivedType(typeof(CallForProposalStatModel))]
    public class CommunityEntityStatModel
    {
        [JsonPropertyName("members_count")]
        public int MemberCount { get; set; }

        [JsonPropertyName("workspaces_count")]
        public int WorkspaceCount { get; set; }

        [JsonPropertyName("cfp_count")]
        public int CFPCount { get; set; }

        [JsonPropertyName("organization_count")]
        public int OrganizationCount { get; set; }

        [JsonPropertyName("hubs_count")]
        public int NodeCount { get; set; }

        [JsonPropertyName("needs_count")]
        public int NeedCount { get; set; }

        [JsonPropertyName("content_entity_count")]
        public int ContentEntityCount { get; set; }

        [JsonPropertyName("participant_count")]
        public int ParticipantCount { get; set; }
    }
}