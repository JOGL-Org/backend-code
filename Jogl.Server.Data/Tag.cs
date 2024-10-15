namespace Jogl.Server.Data
{
    public class Tag : Entity
    {
        public string Text { get; set; }
        public string Description { get; set; }
        public List<string> LinkedIds { get; set; }
        public string Source { get; set; }
    }
}