using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class SpaceRepo : Repo
    {
        public override string Url => $"https://huggingface.co/spaces/{Id}";

        [JsonPropertyName("sdk")]
        public string Sdk { get; set; }

        [JsonPropertyName("likes")]
        public int Likes { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonPropertyName("cardData")]
        public SpaceCardData CardData { get; set; }

        [JsonPropertyName("subdomain")]
        public string Subdomain { get; set; }

        //[JsonPropertyName("gated")]
        //public bool Gated { get; set; }

        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("models")]
        public List<string> Models { get; set; }

        [JsonPropertyName("runtime")]
        public Runtime Runtime { get; set; }

        [JsonPropertyName("siblings")]
        public List<Sibling> Siblings { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("usedStorage")]
        public int UsedStorage { get; set; }

        [JsonIgnore]
        public override string Title => CardData?.Title;

        [JsonIgnore]
        public override string Description => CardData?.ShortDescription;
    }

    public class SpaceCardData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("emoji")]
        public string Emoji { get; set; }

        [JsonPropertyName("colorFrom")]
        public string ColorFrom { get; set; }

        [JsonPropertyName("colorTo")]
        public string ColorTo { get; set; }

        [JsonPropertyName("sdk")]
        public string Sdk { get; set; }

        [JsonPropertyName("sdk_version")]
        public string SdkVersion { get; set; }

        [JsonPropertyName("app_file")]
        public string AppFile { get; set; }

        [JsonPropertyName("pinned")]
        public bool Pinned { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }
    }

    public class DomainData
    {
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("stage")]
        public string Stage { get; set; }
    }

    public class Hardware
    {
        [JsonPropertyName("current")]
        public string Current { get; set; }

        [JsonPropertyName("requested")]
        public string Requested { get; set; }
    }

    public class Replicas
    {
        [JsonPropertyName("current")]
        public int Current { get; set; }

        [JsonPropertyName("requested")]
        public int Requested { get; set; }
    }

    public class Runtime
    {
        [JsonPropertyName("stage")]
        public string Stage { get; set; }

        [JsonPropertyName("hardware")]
        public Hardware Hardware { get; set; }

        [JsonPropertyName("storage")]
        public Storage Storage { get; set; }

        [JsonPropertyName("gcTimeout")]
        public int GcTimeout { get; set; }

        [JsonPropertyName("replicas")]
        public Replicas Replicas { get; set; }

        [JsonPropertyName("devMode")]
        public bool DevMode { get; set; }

        [JsonPropertyName("domains")]
        public List<DomainData> Domains { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }
    }

    public class Storage
    {
        [JsonPropertyName("current")]
        public object Current { get; set; }

        [JsonPropertyName("requested")]
        public object Requested { get; set; }
    }


}