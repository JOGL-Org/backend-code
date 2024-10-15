namespace Jogl.Server.Data
{
    public class Relation : Entity
    {
        public string SourceCommunityEntityId { get; set; }
        public CommunityEntityType SourceCommunityEntityType { get; set; }
        public string TargetCommunityEntityId { get; set; }
        public CommunityEntityType TargetCommunityEntityType { get; set; }

    }
}