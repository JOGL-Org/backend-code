using Jogl.Server.Data.Util;

namespace Jogl.Server.Data
{
    public class Resource : Entity
    {
        public string Title { get; set; }
        [RichText]
        public string Description { get; set; }
        public string FeedId { get; set; }
        public string Type { get; set; }
        public string Condition { get; set; }
        public string ImageId { get; set; }
    }
}