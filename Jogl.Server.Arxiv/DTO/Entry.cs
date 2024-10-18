using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "entry")]
    public class Entry
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "updated")]
        public DateTime Updated { get; set; }

        [XmlElement(ElementName = "published")]
        public DateTime Published { get; set; }

        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "summary")]
        public string Summary { get; set; }

        [XmlElement(ElementName = "author")]
        public List<Author> Author { get; set; }

        [XmlElement(ElementName = "comment")]
        public Comment Comment { get; set; }

        [XmlElement(ElementName = "link")]
        public List<Link> Link { get; set; }

        [XmlElement(ElementName = "primary_category")]
        public PrimaryCategory PrimaryCategory { get; set; }

        [XmlElement(ElementName = "category")]
        public List<Category> Category { get; set; }

        [XmlElement(ElementName = "doi")]
        public Doi Doi { get; set; }

        [XmlElement(ElementName = "journal_ref")]
        public JournalRef JournalRef { get; set; }
    }
}