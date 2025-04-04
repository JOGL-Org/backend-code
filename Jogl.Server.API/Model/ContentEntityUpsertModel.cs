﻿using Jogl.Server.API.Validators;
using Jogl.Server.Data;
using System.Text.Json.Serialization;

namespace Jogl.Server.API.Model
{
    public class ContentEntityUpsertModel
    {
        [JsonIgnore()]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public ContentEntityType Type { get; set; }

        [Obsolete]
        [JsonPropertyName("user_ids")]
        public List<string>? UserIds { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("status")]
        public ContentEntityStatus Status { get; set; }

        [Obsolete]
        [JsonPropertyName("visibility")]
        public ContentEntityVisibility Visibility { get; set; }

        [JsonPropertyName("documents_to_add")]
        [DocumentValidation(DocumentType.Document)]
        public List<DocumentInsertModel>? DocumentsToAdd { get; set; }
    }
}