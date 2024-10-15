using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum ProposalPrivacyLevel { Public, Ecosystem, Private, AdminAndReviewers, Admin }
    public enum DiscussionParticipation { Public, Ecosystem, Private, Participants, AdminOnly }
    [BsonIgnoreExtraElements]
    public class CallForProposal : CommunityEntity
    {
        public string ParentCommunityEntityId { get; set; }
        public PrivacyLevel ProposalParticipation { get; set; }
        public ProposalPrivacyLevel ProposalPrivacy { get; set; }
        public DiscussionParticipation DiscussionParticipation { get; set; }
        public CallForProposalTemplate Template { get; set; }
        public bool Scoring { get; set; }
        public int? MaximumScore { get; set; }
        public DateTime? SubmissionsFrom { get; set; }
        public DateTime? SubmissionsTo { get; set; }
        [RichText]
        public string Rules { get; set; }
        public List<FAQItem> FAQ { get; set; }

        [BsonIgnore]
        public int ProposalCount { get; set; }
        [BsonIgnore]
        public int SubmittedProposalCount { get; set; }

        [BsonIgnore]
        public override CommunityEntityType Type => CommunityEntityType.CallForProposal;

        [BsonIgnore]
        public override FeedType FeedType => FeedType.CallForProposal;
    }

    public class CallForProposalTemplate
    {
        public List<CallForProposalTemplateQuestion> Questions { get; set; }
    }

    public class CallForProposalTemplateSection
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<CallForProposalTemplateQuestion> Questions { get; set; }
    }

    public class CallForProposalTemplateQuestion
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public int? MaxLength { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        [RichText]
        public string Description { get; set; }
        public List<string> Choices { get; set; }
    }
}