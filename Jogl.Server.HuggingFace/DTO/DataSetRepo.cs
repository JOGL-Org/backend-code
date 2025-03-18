using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class DataSetRepo : Repo
    {
        public override string Url => $"https://huggingface.co/datasets/{Id}";

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonPropertyName("private")]
        public bool Private { get; set; }

        [JsonPropertyName("gated")]
        public bool Gated { get; set; }

        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("citation")]
        public object Citation { get; set; }

        [JsonPropertyName("description")]
        public string DataSetDescription { get; set; }

        [JsonPropertyName("downloads")]
        public int Downloads { get; set; }

        [JsonPropertyName("likes")]
        public int Likes { get; set; }

        [JsonPropertyName("cardData")]
        public DatasetCardData CardData { get; set; }

        [JsonPropertyName("siblings")]
        public List<Sibling> Siblings { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("usedStorage")]
        public int UsedStorage { get; set; }

        [JsonIgnore]
        public override string Title => Id;
        
        [JsonIgnore]
        public override string Description => DataSetDescription;
    }

    public class DatasetCardData
    {
        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("task_categories")]
        public List<string> TaskCategories { get; set; }

        [JsonPropertyName("language")]
        public List<string> Language { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("configs")]
        public List<Config> Configs { get; set; }
    }

    public class Config
    {
        [JsonPropertyName("config_name")]
        public string ConfigName { get; set; }

        [JsonPropertyName("data_files")]
        public string DataFiles { get; set; }
    }
}