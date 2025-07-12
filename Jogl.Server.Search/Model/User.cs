using System.Text.Json.Serialization;

namespace Jogl.Server.Search.Model
{
    public class User : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string ShortBio { get; set; }
        public string Bio { get; set; }
        public string Current { get; set; }
        public string Location { get; set; }
        public List<string> Current_Roles { get; set; }
        public List<string> Past_Roles { get; set; }
        public List<string> Current_Companies { get; set; }
        public List<string> Past_Companies { get; set; }
        public List<string> Study_Programs { get; set; }
        public List<string> Study_Institutions { get; set; }
        public List<string> Labels { get; set; }
        public List<string> Documents_Title { get; set; }
        public List<string> Documents_Content { get; set; }
    }
}
