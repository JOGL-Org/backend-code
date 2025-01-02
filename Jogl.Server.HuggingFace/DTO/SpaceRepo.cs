namespace Jogl.Server.HuggingFace.DTO
{
    public class SpaceRepo : Repo
    {
        public override string Url => $"https://huggingface.co/spaces/{Id}";
    }
}