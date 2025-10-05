namespace Jogl.Server.Data
{
    public enum FeedType { Project, Workspace, Node, Need, User, Organization, Paper, Document, CallForProposal, Event, Channel, Resource }
    public class Feed : Entity
    {
        public FeedType Type { get; set; }
    }
}