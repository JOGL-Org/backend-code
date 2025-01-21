using Jogl.Server.Data.Util;

namespace Jogl.Server.Data
{
    public class Discussion : ListPage<ContentEntity>
    {
        public Discussion(IEnumerable<ContentEntity> items) : base(items)
        {
        }

        public Discussion(IEnumerable<ContentEntity> items, int total) : base(items, total)
        {
        }

        public DiscussionStats DiscussionStats { get; set; }

        public FeedEntity FeedEntity { get; set; }
        public FeedEntity ParentFeedEntity { get; set; }
    }
}