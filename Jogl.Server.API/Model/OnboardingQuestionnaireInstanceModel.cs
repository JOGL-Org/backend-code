using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingQuestionnaireInstanceModel 
    {
        [JsonPropertyName("items")]
        public List<OnboardingQuestionnaireInstanceItemModel> Items { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTime CompletedUTC { get; set; }
    }
}