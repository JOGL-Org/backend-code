using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingPresentationModel
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        [JsonPropertyName("items")]
        public List<OnboardingPresentationItemModel> Items { get; set; }
    }
}