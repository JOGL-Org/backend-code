using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum ChannelVisibility { Private, Open }

    [BsonIgnoreExtraElements]
    public class Channel : FeedEntity, ICommunityEntityOwned
    {
        public string CommunityEntityId { get; set; }

        public string IconKey { get; set; }

        public string Title { get; set; }
        public string Key { get; set; }

        public string? Description { get; set; }

        public ChannelVisibility Visibility { get; set; }

        public bool AutoJoin { get; set; }

        public List<string> Settings { get; set; }

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Channel;

        [BsonIgnore]
        public override string FeedTitle => Title;

        [BsonIgnore]
        public CommunityEntity CommunityEntity { get; set; }

        [BsonIgnore]
        public int MemberCount { get; set; }

        [BsonIgnore]
        public AccessLevel? CurrentUserAccessLevel { get; set; }

        [BsonIgnore]
        public int UnreadPosts { get; set; }

        [BsonIgnore]
        public int UnreadMentions { get; set; }

        [BsonIgnore]
        public int UnreadThreads { get; set; }

        [BsonIgnore]
        public List<Membership> Members { get; set; }
    }
}