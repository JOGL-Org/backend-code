namespace Jogl.Server.HuggingFace.DTO
{
    public class ModelRepo : Repo
    {
        public override string Url => $"https://huggingface.co/{Id}";
    }
}