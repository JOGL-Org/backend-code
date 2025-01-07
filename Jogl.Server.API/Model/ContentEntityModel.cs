using Jogl.Server.Data;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ContentEntityModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public ContentEntityType Type { get; set; }

        [JsonPropertyName("users")]
        public List<UserMiniModel> Users { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("status")]
        public ContentEntityStatus Status { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("overrides")]
        public ContentEntityOverridesModel? Overrides { get; set; }

        [JsonPropertyName("feed_entity")]
        public EntityMiniModel? FeedEntity { get; set; }

        [JsonPropertyName("parent_feed_entity")]
        public EntityMiniModel? ParentFeedEntity { get; set; }

        [JsonPropertyName("documents")]
        public List<DocumentModel> Documents { get; set; }

        [JsonIgnore]
        public List<ReactionModel> Reactions { get; set; }

        [JsonPropertyName("reaction_count")]
        public List<ReactionCountModel> ReactionCounts
        {
            get
            {
                if (Reactions == null)
                    return new List<ReactionCountModel>();

                return Reactions
                    .GroupBy(r => r.Key)
                    .Select(grp => new ReactionCountModel { Key = grp.Key, Count = grp.Count() })
                    .OrderByDescending(grp => grp.Count)
                    .ToList();
            }
        }

        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }

        [JsonPropertyName("new_comment_count")]
        public int NewCommentCount { get; set; }

        [JsonPropertyName("user_reaction")]
        public ReactionModel UserReaction { get; set; }

        [JsonPropertyName("user_mentions")]
        public int UserMentions { get; set; }

        [JsonPropertyName("user_mentions_in_comments")]
        public int UserMentionsInComments { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("image_url_sm")]
        public string ImageUrlSmall { get; set; }

        [JsonPropertyName("feed_stats")]
        public FeedStatModel FeedStats { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

        [JsonPropertyName("last_comment")]
        public CommentModel LastComment { get; set; }

        [JsonPropertyName("user_source")]
        public ContentEntitySource UserSource { get; set; }
    }
}