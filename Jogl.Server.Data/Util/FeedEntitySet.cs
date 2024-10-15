namespace Jogl.Server.Data.Util
{
    public class FeedEntitySet
    {
        public List<Workspace> Communities { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<CallForProposal> CallsForProposals { get; set; }

        public List<Need> Needs { get; set; }
        public List<Document> Documents { get; set; }
        public List<Paper> Papers { get; set; }
        public List<Event> Events { get; set; }

        public List<User> Users { get; set; }
        public List<Channel> Channels { get; set; }


        public List<FeedEntity> FeedEntities
        {
            get
            {
                var res = new List<FeedEntity>();
                res.AddRange(Communities);
                res.AddRange(Nodes);
                res.AddRange(Organizations);
                res.AddRange(CallsForProposals);
                res.AddRange(Needs);
                res.AddRange(Papers);
                res.AddRange(Events);
                res.AddRange(Users);
                res.AddRange(Channels);

                return res;
            }
        }

        public List<CommunityEntity> CommunityEntities
        {
            get
            {
                var res = new List<CommunityEntity>();
                res.AddRange(Communities);
                res.AddRange(Nodes);
                res.AddRange(Organizations);
                res.AddRange(CallsForProposals);

                return res;
            }
        }

        public FeedEntitySet()
        {
            Communities = new List<Workspace>();
            Nodes = new List<Node>();
            Organizations = new List<Organization>();
            CallsForProposals = new List<CallForProposal>();

            Needs = new List<Need>();
            Documents = new List<Document>();
            Papers = new List<Paper>();
            Events = new List<Event>();
            Users = new List<User>();
            Channels = new List<Channel>();
        }
    }
}
