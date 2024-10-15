using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum ProposalStatus { Draft, Submitted, Rejected, Accepted }
    public class Proposal : Entity
    {
        public string SourceCommunityEntityId { get; set; }
        public string CallForProposalId { get; set; }
        public string Title { get; set; }
        [RichText]
        public string Description { get; set; }
        public ProposalStatus Status { get; set; }
        public List<string> UserIds { get; set; }
        public List<ProposalAnswer> Answers { get; set; }
        public decimal Score { get; set; }
        public DateTime? Submitted { get; set; }

        [BsonIgnore]
        public CommunityEntity SourceCommunityEntity { get; set; }

        [BsonIgnore]
        public CallForProposal CallForProposal { get; set; }

        [BsonIgnore]
        public CommunityEntity ParentCommunityEntity { get; set; }

        [BsonIgnore]
        public List<User> Users { get; set; }

    }

    public class ProposalAnswer
    {
        public string QuestionKey { get; set; }
        [RichText]
        public List<string> Answer { get; set; }
        public string AnswerDocumentId { get; set; }

        [BsonIgnore]
        public Document AnswerDocument { get; set; }
    }
}