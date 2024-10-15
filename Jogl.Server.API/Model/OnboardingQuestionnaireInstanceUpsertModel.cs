using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class OnboardingQuestionnaireInstanceUpsertModel
    {
        [JsonPropertyName("items")]
        public List<OnboardingQuestionnaireInstanceItemModel> Items { get; set; }
    }
}