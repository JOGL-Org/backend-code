using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class CommunityModel : CommunityEntityModel
    {
        [JsonPropertyName("feed_open")]
        public bool FeedOpen { get; set; }

        [JsonPropertyName("onboarding")]
        public OnboardingConfigurationModel Onboarding { get; set; }

        [JsonPropertyName("faq")]
        public List<FAQItem> FAQ { get; set; }

        [JsonPropertyName("locations")]
        public List<string> Locations { get; set; }
    }
}