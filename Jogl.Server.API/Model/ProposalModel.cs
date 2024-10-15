using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProposalModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("project")]
        public EntityMiniModel SourceCommunityEntity { get; set; }

        [JsonPropertyName("call_for_proposal")]
        public CallForProposalMiniModel CallForProposal { get; set; }

        [JsonPropertyName("community")]
        public EntityMiniModel ParentCommunityEntity { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("status")]
        public ProposalStatus Status { get; set; }

        [JsonPropertyName("users")]
        public List<UserMiniModel> Users { get; set; }

        [JsonPropertyName("answers")]
        public List<ProposalAnswerModel> Answers { get; set; }

        [JsonPropertyName("score")]
        public decimal Score { get; set; }

        [JsonPropertyName("submitted_at")]
        public DateTime? Submitted { get; set; }
    }

    public class ProposalAnswerModel
    {
        [JsonPropertyName("question_key")]
        public string QuestionKey { get; set; }

        [JsonPropertyName("answer")]
        public List<string> Answer { get; set; }

        [JsonPropertyName("answer_document")]
        public DocumentModel AnswerDocument { get; set; }
    }
}