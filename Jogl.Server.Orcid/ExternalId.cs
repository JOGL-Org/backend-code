namespace Jogl.Server.Orcid
{

    public class ExternalId
    {
        public string UrlType { get; set; }
        public string UrlValue { get; set; }
        public string Url { get; set; }

        public string Relationship { get; set; }

        public ExternalId()
        {
            UrlType = "";
            UrlValue = "";
            Url = "";
            Relationship = "";
        }

    }
}
