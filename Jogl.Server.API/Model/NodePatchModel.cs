using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class NodePatchModel : CommunityEntityPatchModel
    {
        [JsonPropertyName("onboarding")]
        public OnboardingConfigurationUpsertModel? Onboarding { get; set; }

        [JsonPropertyName("faq")]
        public List<FAQItem>? FAQ { get; set; }

        [JsonPropertyName("external_website")]
        public string? Website { get; set; }
    }
}