using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ProposalUpsertModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("project_id")]
        public string SourceCommunityEntityId { get; set; }

        [JsonPropertyName("call_for_proposal_id")]
        public string CallForProposalId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("answers")]
        public List<ProposalAnswerUpsertModel>? Answers { get; set; }
    }

    public class ProposalAnswerUpsertModel
    {
        [JsonPropertyName("question_key")]
        public string QuestionKey { get; set; }

        [JsonPropertyName("answer")]
        public List<string> Answer { get; set; }

        [JsonPropertyName("answer_document_id")]
        public string AnswerDocumentId { get; set; }
    }
}