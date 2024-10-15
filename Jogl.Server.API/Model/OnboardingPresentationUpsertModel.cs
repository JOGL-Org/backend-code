using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingPresentationUpsertModel
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        [JsonPropertyName("items")]
        public List<OnboardingPresentationItemUpsertModel> Items { get; set; }
    }
}