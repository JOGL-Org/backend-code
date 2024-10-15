namespace Jogl.Server.Orcid
{
    public class Person
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string Biography { get; set; }
        public List<ResearcherWebsite> ResearcherUrls { get; set; }
        public List<string> Emails { get; set; }

        public List<string> Country { get; set; }
        public List<string> Keywords { get; set; }
        public List<OtherIds> ExternalIds { get; set; }

        public Person()
        {
            GivenName = "";
            FamilyName = "";
            Biography = "";
            ResearcherUrls = new List<ResearcherWebsite>();
            Emails = new List<string>();
            Country = new List<string>();
            Keywords = new List<string>();
            ExternalIds = new List<OtherIds>();
        }
    }
}
