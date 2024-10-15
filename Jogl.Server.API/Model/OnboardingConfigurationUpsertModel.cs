using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingConfigurationUpsertModel
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("presentation")]
        public OnboardingPresentationUpsertModel Presentation { get; set; }
     
        [JsonPropertyName("questionnaire")]
        public OnboardingQuestionnaireModel Questionnaire { get; set; }
    
        [JsonPropertyName("rules")]
        public OnboardingRulesModel Rules { get; set; }
    }
}