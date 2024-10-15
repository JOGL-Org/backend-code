using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class Discussion
    {
        [JsonPropertyName("num")]
        public int Num { get; set; }

        [JsonPropertyName("author")]
        public User Author { get; set; }

        [JsonPropertyName("repo")]
        public Repo Repo { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("isPullRequest")]
        public bool IsPullRequest { get; set; }

        [JsonPropertyName("numComments")]
        public int NumComments { get; set; }

        [JsonPropertyName("pinned")]
        public bool Pinned { get; set; }
    }

    public class PullRequestResponse
    {
        [JsonPropertyName("discussions")]
        public List<Discussion> Discussions { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("numClosedDiscussions")]
        public int NumClosedDiscussions { get; set; }
    }


}