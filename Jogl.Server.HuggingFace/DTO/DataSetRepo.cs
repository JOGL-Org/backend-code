namespace Jogl.Server.HuggingFace.DTO
{
    public class DataSetRepo : Repo
    {
        public override string Url => $"https://huggingface.co/datasets/{Id}";
    }
}