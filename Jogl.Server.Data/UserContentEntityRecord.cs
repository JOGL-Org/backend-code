﻿namespace Jogl.Server.Data
{
    public class UserContentEntityRecord : Entity
    {
        public string UserId { get; set; }
        public string FeedId { get; set; }
        public string ContentEntityId { get; set; }
        public DateTime? LastReadUTC { get; set; }
        public DateTime? LastWriteUTC { get; set; }
        public DateTime? LastMentionUTC { get; set; }
    }
}