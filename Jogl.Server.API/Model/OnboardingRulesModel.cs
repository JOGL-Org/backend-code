using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingRulesModel
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}