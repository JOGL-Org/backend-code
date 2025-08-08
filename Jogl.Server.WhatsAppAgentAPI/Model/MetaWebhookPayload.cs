using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

public class MetaWebhookPayload
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<WebhookEntry> Entry { get; set; } = new();
}

public class WebhookEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("changes")]
    public List<WebhookChange> Changes { get; set; } = new();
}

public class WebhookChange
{
    [JsonPropertyName("value")]
    public WebhookValue Value { get; set; } = new();

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public class WebhookValue
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public WebhookMetadata Metadata { get; set; } = new();

    [JsonPropertyName("contacts")]
    public List<Contact> Contacts { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<WhatsAppMessage> Messages { get; set; } = new();

    [JsonPropertyName("statuses")]
    public List<MessageStatus> Statuses { get; set; } = new();
}

public class WebhookMetadata
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class Contact
{
    [JsonPropertyName("profile")]
    public ContactProfile Profile { get; set; } = new();

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class ContactProfile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class WhatsAppMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public TextMessage? Text { get; set; }

    [JsonPropertyName("image")]
    public MediaMessage? Image { get; set; }

    [JsonPropertyName("document")]
    public MediaMessage? Document { get; set; }

    [JsonPropertyName("audio")]
    public MediaMessage? Audio { get; set; }

    [JsonPropertyName("video")]
    public MediaMessage? Video { get; set; }
}

public class TextMessage
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class MediaMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}

public class MessageStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;
}

public class MetaApiResponse
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("contacts")]
    public List<ApiContact> Contacts { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<ApiMessage> Messages { get; set; } = new();
}

public class ApiContact
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class ApiMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class WebhookVerificationRequest
{
    [FromQuery(Name = "hub.mode")]
    public string Mode { get; set; } = string.Empty;

    [FromQuery(Name = "hub.verify_token")]
    public string VerifyToken { get; set; } = string.Empty;

    [FromQuery(Name = "hub.challenge")]
    public string Challenge { get; set; } = string.Empty;
}

// Request Models
public class SendMessageRequest
{
    public string To { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}