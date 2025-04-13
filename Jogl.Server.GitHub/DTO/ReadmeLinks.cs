using System.Text.Json.Serialization;

public class ReadmeLinks
{
    [JsonPropertyName("self")]
    public string Self { get; set; }

    [JsonPropertyName("git")]
    public string Git { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }
}

