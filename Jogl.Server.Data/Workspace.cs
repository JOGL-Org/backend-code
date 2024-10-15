using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    [BsonIgnoreExtraElements]
    public class Workspace : CommunityEntity
    {
        public List<FAQItem> FAQ { get; set; }

        [JsonPropertyName("locations")]
        public List<string> Locations { get; set; }

        public string Label { get; set; }

        [BsonIgnore]
        public override CommunityEntityType Type => CommunityEntityType.Workspace;

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Workspace;

    }
}