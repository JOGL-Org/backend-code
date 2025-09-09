using Jogl.Server.Data.Util;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum ResourceType
    {
        Repository, Patent
    }

    [BsonIgnoreExtraElements]
    public class Resource : FeedEntity
    {
        public string Title { get; set; }

        [RichText]
        public string Description { get; set; }

        [JsonConverter(typeof(BsonDocumentConverter))]
        public BsonDocument Data { get; set; }

        public string EntityId { get; set; }

        public ResourceType Type { get; set; }

        public override string FeedTitle => Title;

        public override FeedType FeedType => FeedType.Resource;
        public string this[string key]
        {
            get
            {
                if (Data == null)
                    return null;

                return Data.Contains(key) ? Data[key].ToString() : null;
            }
        }
    }
}
