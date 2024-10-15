using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class Project : CommunityEntity
    {
        public string Maturity { get; set; }

        public bool IsLookingForCollaborators { get; set; }

        [BsonIgnore]
        public override CommunityEntityType Type => CommunityEntityType.Project;

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Project;

        [BsonIgnore]
        public int ProposalCount { get; set; }
    }
}