namespace Jogl.Server.Orcid
{
    public class Work
    {
        public string SourceName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string WorkType { get; set; }
        public string PublicationDate { get; set; }
        public string JournalTitle { get; set; }

        public List<Contributor> Contributors { get; set; }

        public List<ExternalId> ExternalIds { get; set; }
        
        public List<string> Tags { get; set; }

        public Work()
        {
            Title = "";
            Description = "";
            WorkType = "";
            SourceName = "";
            PublicationDate = "";
            JournalTitle = "";
            Contributors = new List<Contributor>();
            ExternalIds = new List<ExternalId>();
            Tags = new List<string>();
        }
    }
}
