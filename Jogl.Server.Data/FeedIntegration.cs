﻿using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public enum FeedIntegrationType { GitHub, HuggingFace, Arxiv }

    [BsonIgnoreExtraElements]
    public class FeedIntegration : Entity
    {
        public string FeedId { get; set; }

        public FeedIntegrationType Type { get; set; }

        public string SourceId { get; set; }
        
        public string SourceUrl { get; set; }
        
        public string? AccessToken { get; set; }
    }
}