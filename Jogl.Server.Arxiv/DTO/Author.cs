using System.Xml.Serialization;

namespace Jogl.Server.Arxiv.DTO
{
    [XmlRoot(ElementName = "author")]
    public class Author
    {

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
    }
}