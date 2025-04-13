using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;

namespace Jogl.Server.HuggingFace.DTO
{
    public class ModelRepo : Repo
    {
        public override string Url => $"https://huggingface.co/{Id}";

        [JsonPropertyName("private")]
        public bool Private { get; set; }

        [JsonPropertyName("pipeline_tag")]
        public string PipelineTag { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("downloads")]
        public int Downloads { get; set; }

        [JsonPropertyName("likes")]
        public int Likes { get; set; }

        [JsonPropertyName("modelId")]
        public string ModelId { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; }

        //[JsonPropertyName("gated")]
        //public bool Gated { get; set; }

        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; }

        [JsonPropertyName("model-index")]
        public object ModelIndex { get; set; }

        [JsonPropertyName("cardData")]
        public ModelCardData CardData { get; set; }

        [JsonPropertyName("siblings")]
        public List<Sibling> Siblings { get; set; }

        [JsonPropertyName("spaces")]
        public List<string> Spaces { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("safetensors")]
        //public Safetensors Safetensors { get; set; }

        //[JsonPropertyName("usedStorage")]
        //public long UsedStorage { get; set; }

        [JsonIgnore]
        public override string Title => Id;

        [JsonIgnore]
        public override string Description => "";
    }

    public class ModelCardData
    {
        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("language")]
        public List<string> Language { get; set; }

        [JsonPropertyName("pipeline_tag")]
        public string PipelineTag { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }

    //public class Parameters
    //{
    //    [JsonPropertyName("F32")]
    //    public int F32 { get; set; }
    //}

    //public class Safetensors
    //{
    //    [JsonPropertyName("parameters")]
    //    public Parameters Parameters { get; set; }

    //    [JsonPropertyName("total")]
    //    public int Total { get; set; }
    //}

}