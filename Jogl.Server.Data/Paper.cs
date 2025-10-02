using Jogl.Server.Data.Util;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public enum PaperType { Article, Preprint, Note }
    public enum PaperTag { Reference, Aggregated, Library, AuthoredByMe, AuthorOf, Workspace, Node }
    public enum ExternalSystem
    {
        ORCID, SemanticScholar, OpenAlex, Pubmed, None
    }

    public enum Source
    {
        SEMANTIC, CROSSREF, DATACITE, PUBMED
    }

    [BsonIgnoreExtraElements]
    public class Paper : FeedEntity, IFeedEntityOwned
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Authors { get; set; }
        public string FeedId { get; set; }
        public List<string> UserIds { get; set; }
        public string Journal { get; set; }
        public string OpenAccessPdfUrl { get; set; }
        public string ExternalId { get; set; }
        public string SourceId { get; set; }
        public PaperType Type { get; set; }
        public ContentEntityStatus Status { get; set; }
        public ExternalSystem ExternalSystem { get; set; }
        public string PublicationDate { get; set; }

        public TagData TagData { get; set; }

        [BsonIgnore]
        public List<User> Users { get; set; }

        [BsonIgnore]
        public int NewPostCount { get; set; }

        [BsonIgnore]
        public int NewMentionCount { get; set; }

        [BsonIgnore]
        public int NewThreadActivityCount { get; set; }

        [BsonIgnore]
        public int CommentCount { get; set; }

        [BsonIgnore]
        public override FeedType FeedType => FeedType.Paper;

        [BsonIgnore]
        public override string FeedTitle => Title;

        [BsonIgnore]
        [JsonIgnore]
        public FeedEntity FeedEntity { get; set; }

        public string FeedEntityId => FeedId;
    }

    public class TagData
    {
        public List<string> DOITags { get; set; }
        public List<SemanticTag> S2Tags { get; set; }
        public List<PubMedTag> PubMedTags { get; set; }
        public List<OpenAlexTag> OpenAlexTags { get; set; }
    }

    public class StringTag
    {
        public string Value { get; set; }
    }

    public class SemanticTag
    {
        public string Category { get; set; }
        public string Source { get; set; }
    }

    public class PubMedTag
    {
        public string DescriptorName { get; set; }
        public string[] QualifierNames { get; set; }
    }

    public class OpenAlexTag
    {
        public string Id { get; set; }
        public string Wikidata { get; set; }
        public string DisplayName { get; set; }
        public int Level { get; set; }
        public double Score { get; set; }

    }
}