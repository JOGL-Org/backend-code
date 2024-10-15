namespace Jogl.Server.Data
{
    public class Folder : Entity
    {
        public string Name { get; set; }
        public string FeedId { get; set; }
        public string ParentFolderId { get; set; }
    }
}