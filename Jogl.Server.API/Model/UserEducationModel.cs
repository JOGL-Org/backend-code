﻿using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class UserEducationModel
    {
        [JsonPropertyName("school")]
        public string Institution { get; set; }
        [JsonPropertyName("program")]
        public string? Program { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("date_from")]
        public string? DateFrom { get; set; }
        [JsonPropertyName("date_to")]
        public string? DateTo { get; set; }
        [JsonPropertyName("current")]
        public bool Current { get; set; }
    }
}