using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CallForProposalModel : CommunityEntityModel
    {
        [JsonPropertyName("community_id")]
        public string ParentCommunityEntityId { get; set; }

        [JsonPropertyName("proposal_participation")]
        public PrivacyLevel ProposalParticipation { get; set; }

        [JsonPropertyName("proposal_privacy")]
        public ProposalPrivacyLevel ProposalPrivacy { get; set; }

        [JsonPropertyName("discussion_participation")]
        public DiscussionParticipation DiscussionParticipation { get; set; }

        [JsonPropertyName("template")]
        public CallForProposalTemplateModel Template { get; set; }

        [JsonPropertyName("scoring")]
        public bool Scoring { get; set; }

        [JsonPropertyName("max_score")]
        public int MaximumScore { get; set; }

        [JsonPropertyName("submissions_from")]
        public DateTime? SubmissionsFrom { get; set; }

        [JsonPropertyName("submissions_to")]
        public DateTime? SubmissionsTo { get; set; }

        [JsonPropertyName("rules")]
        public string Rules { get; set; }

        [JsonPropertyName("faq")]
        public List<FAQItem> FAQ { get; set; }

        [JsonIgnore]
        public override CommunityEntityStatModel Stats { get; set; }

        [JsonPropertyName("stats")]
        public virtual CallForProposalStatModel CFPStats { get; set; }
    }

    public class CallForProposalTemplateModel
    {
        [JsonPropertyName("questions")]
        public List<CallForProposalTemplateQuestionModel> Questions { get; set; }
    }

    public class CallForProposalTemplateSectionModel
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("questions")]
        public List<CallForProposalTemplateQuestionModel> Questions { get; set; }
    }

    public class CallForProposalTemplateQuestionModel
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("max_length")]
        public int? MaxLength { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("choices")]
        public List<string> Choices { get; set; }
    }
}