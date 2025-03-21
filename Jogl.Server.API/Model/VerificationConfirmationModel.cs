﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class VerificationConfirmationModel
    {
        [JsonPropertyName("email")]
        [EmailAddress]
        public string Email { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}