namespace Jogl.Server.Data
{
    public enum AccessOriginType { Public, EcosystemMember, DirectMember }
    public class AccessOrigin
    {
        public AccessOrigin()
        {
            EcosystemMemberships = new List<Membership>();
        }

        public AccessOrigin(AccessOriginType type)
        {
            Type = type;
            EcosystemMemberships = new List<Membership>();
        }

        public AccessOrigin(AccessOriginType type, List<Membership> ecosystemMemberships)
        {
            Type = type;
            EcosystemMemberships = ecosystemMemberships;
        }

        public AccessOriginType Type { get; set; }
        public List<Membership> EcosystemMemberships { get; set; }
    }
}