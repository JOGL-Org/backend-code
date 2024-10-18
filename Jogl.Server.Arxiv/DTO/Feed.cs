using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "feed", Namespace = "http://www.w3.org/2005/Atom")]
    public class Feed
    {
        [XmlElement(ElementName = "link")]
        public Link Link { get; set; }

        [XmlElement(ElementName = "title")]
        public Title Title { get; set; }

        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "updated")]
        public DateTime Updated { get; set; }

        [XmlElement(ElementName = "totalResults")]
        public TotalResults TotalResults { get; set; }

        [XmlElement(ElementName = "startIndex")]
        public StartIndex StartIndex { get; set; }

        [XmlElement(ElementName = "itemsPerPage")]
        public ItemsPerPage ItemsPerPage { get; set; }

        [XmlElement(ElementName = "entry")]
        public List<Entry> Entry { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}