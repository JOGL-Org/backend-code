﻿using Jogl.Server.Data.Util;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum ResourceType
    {
        Repository
    }

    [BsonIgnoreExtraElements]
    public class Resource : FeedEntity
    {
        public string Title { get; set; }

        [RichText]
        public string Description { get; set; }

        public BsonDocument Data { get; set; }

        public string EntityId { get; set; }

        public ResourceType Type { get; set; }

        public override string FeedTitle => Title;

        public override FeedType FeedType => FeedType.Resource;
    }
}