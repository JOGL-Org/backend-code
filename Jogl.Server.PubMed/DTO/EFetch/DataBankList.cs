using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
    [XmlRoot(ElementName = "DataBankList")]
    public class DataBankList
    {
        [XmlAttribute(AttributeName = "CompleteYN")]
        public string CompleteYN { get; set; }

        [XmlElement(ElementName = "DataBank")]
        public List<DataBank> DataBank { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
