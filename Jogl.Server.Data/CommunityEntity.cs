using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum CommunityEntityType { Project, Workspace, Node, Organization, CallForProposal, Channel }
    public enum PrivacyLevel { Public, Ecosystem, Private, Custom }
    public enum JoiningRestrictionLevel { Invite, Request, Open, Custom }

    public abstract class CommunityEntity : FeedEntity
    {
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        [RichText]
        public string Description { get; set; }
        [RichText]
        public string ShortDescription { get; set; }
        public string FeedId { get; set; }
        public string BannerId { get; set; }
        public string LogoId { get; set; }
        public PrivacyLevel ListingPrivacy { get; set; }
        public PrivacyLevel ContentPrivacy { get; set; }
        public List<PrivacyLevelSetting> ContentPrivacyCustomSettings { get; set; }
        public JoiningRestrictionLevel JoiningRestrictionLevel { get; set; }
        public List<JoiningRestrictionLevelSetting> JoiningRestrictionLevelCustomSettings { get; set; }
        public string Status { get; set; }
        public List<string> Interests { get; set; }
        public List<string> Keywords { get; set; }
        public List<Link> Links { get; set; }
        public List<string> Tabs { get; set; }
        public List<string> Settings { get; set; }
        public string? HomeChannelId { get; set; }

        public OnboardingConfiguration Onboarding { get; set; }

        [BsonIgnore]
        public int WorkspaceCount { get; set; }

        [BsonIgnore]
        public int NodeCount { get; set; }

        [BsonIgnore]
        public int OrganizationCount { get; set; }

        [BsonIgnore]
        public int CFPCount { get; set; }

        [BsonIgnore]
        public int NeedCount { get; set; }

        [BsonIgnore]
        public int NeedCountAggregate { get; set; }

        [BsonIgnore]
        public int DocumentCount { get; set; }

        [BsonIgnore]
        public int DocumentCountAggregate { get; set; }

        [BsonIgnore]
        public int PaperCount { get; set; }

        [BsonIgnore]
        public int PaperCountAggregate { get; set; }

        [BsonIgnore]
        public int ResourceCount { get; set; }

        [BsonIgnore]
        public int ResourceCountAggregate { get; set; }

        [BsonIgnore]
        public int MemberCount { get; set; }

        [BsonIgnore]
        public int ContentEntityCount { get; set; }

        [BsonIgnore]
        public int ParticipantCount { get; set; }

        [BsonIgnore]
        public int PostCount { get; set; }

        [BsonIgnore]
        public AccessLevel? AccessLevel { get; set; }

        [BsonIgnore]
        public AccessOrigin? ListingAccessOrigin { get; set; }

        [BsonIgnore]
        public AccessOrigin? ContentAccessOrigin { get; set; }

        [BsonIgnore]
        public DateTime? OnboardedUTC { get; set; }

        [BsonIgnore]
        public string Contribution { get; set; }

        [BsonIgnore]
        public abstract CommunityEntityType Type { get; }

        [BsonIgnore]
        public override string FeedTitle => Title;

        [BsonIgnore]
        public override string FeedLogoId => LogoId;

        [BsonIgnore]
        public List<Channel> Channels { get; set; }

        [BsonIgnore]
        public int Level { get; set; }
    }
}