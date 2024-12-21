using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum NeedType
    {
        Funding,
        Equipment,
        Expertise,
        SoftwareLicense,
        OtherLicense,
        Tasks
    }

    [BsonIgnoreExtraElements]
    public class Need : FeedEntity, ICommunityEntityOwned, IFeedEntityOwned
    {
        public string Title { get; set; }
        [RichText]
        public string Description { get; set; }
        public string EntityId { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Interests { get; set; }
        public List<string> Skills { get; set; }
        public NeedType Type { get; set; }

        [BsonIgnore]
        public int PostCount { get; set; }

        [BsonIgnore]
        public int NewPostCount { get; set; }

        [BsonIgnore]
        public int NewMentionCount { get; set; }

        [BsonIgnore]
        public int NewThreadActivityCount { get; set; }


        [BsonIgnore]
        public int CommentCount { get; set; }

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Need;

        [BsonIgnore]
        public override string FeedTitle => Title;

        [BsonIgnore]
        public CommunityEntity CommunityEntity { get; set; }

        [BsonIgnore]
        public string CommunityEntityId { get => EntityId; }

        [BsonIgnore]
        public FeedEntity FeedEntity { get => CommunityEntity; set { CommunityEntity = value as CommunityEntity; } }

        [BsonIgnore]
        public string FeedEntityId { get => EntityId; }
    }
}