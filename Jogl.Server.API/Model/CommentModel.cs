using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommentModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("reply_to_id")]
        public string ReplyToId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("created_by")]
        public UserMiniModel? CreatedBy { get; set; }

        [JsonPropertyName("overrides")]
        public DiscussionItemOverridesModel? Overrides { get; set; }

        [JsonPropertyName("user_mentions")]
        public int UserMentions { get; set; }

        [JsonPropertyName("documents")]
        public List<DocumentModel> Documents { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }

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

        [JsonPropertyName("user_reaction")]
        public ReactionModel UserReaction { get; set; }
    }
}