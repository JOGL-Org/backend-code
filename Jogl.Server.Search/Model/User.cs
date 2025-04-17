using System.Text.Json.Serialization;

namespace Jogl.Server.Search.Model
{
    public class User : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ShortBio { get; set; }
        public string Bio { get; set; }
        public List<string> Educations_Institution { get; set; }
        public List<string> Educations_Program { get; set; }
        public List<string> Experiences_Company { get; set; }
        public List<string> Experiences_Position { get; set; }
        public List<string> Documents_Title { get; set; }
        public List<string> Documents_Content { get; set; }
        public List<string> Papers_Title { get; set; }
        public List<string> Papers_Abstract { get; set; }

    }
}
