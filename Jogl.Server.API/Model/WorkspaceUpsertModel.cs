﻿using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class WorkspaceUpsertModel : CommunityEntityUpsertModel
    {
        [JsonPropertyName("onboarding")]
        public OnboardingConfigurationUpsertModel? Onboarding { get; set; }

        [JsonPropertyName("faq")]
        public List<FAQItem>? FAQ { get; set; }

        [JsonPropertyName("locations")]
        public List<string>? Locations { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; }
    }
}