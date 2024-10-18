using MongoDB.Bson.Serialization.Attributes;

namespace Jogl.Server.Data
{
    public class Publication : Entity
    {
        public string DOI { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ExternalSystem { get; set; }
        public string ExternalID { get; set; }
        public string ExternalURL { get; set; }
        public string ExternalFileURL { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Tags { get; set; }
        public DateTime? Published { get; set; }
        public string Submitter { get; set; }
        public string LicenseURL { get; set; }
        public string Journal { get; set; }
    }
}
