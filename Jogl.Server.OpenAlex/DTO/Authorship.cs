using System.Text.Json.Serialization;

namespace Jogl.Server.OpenAlex.DTO
{
    public class Authorship
    {
        [JsonPropertyName("author_position")]
        public string AuthorPosition { get; set; }

        [JsonPropertyName("author")]
        public Author Author { get; set; }

        [JsonPropertyName("institutions")]
        public List<Institution> Institutions { get; set; }

        [JsonPropertyName("countries")]
        public List<string> Countries { get; set; }

        [JsonPropertyName("is_corresponding")]
        public bool IsCorresponding { get; set; }

        [JsonPropertyName("raw_author_name")]
        public string RawAuthorName { get; set; }

        [JsonPropertyName("raw_affiliation_string")]
        public string RawAffiliationString { get; set; }

        [JsonPropertyName("raw_affiliation_strings")]
        public List<string> RawAffiliationStrings { get; set; }
    }
}
