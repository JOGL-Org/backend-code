using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
    [XmlRoot(ElementName = "InvestigatorList")]
    public class InvestigatorList
    {
        [XmlElement(ElementName = "Investigator")]
        public List<Investigator> Investigator { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
