using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
    [XmlRoot(ElementName = "DataBank")]
    public class DataBank
    {
        [XmlElement(ElementName = "DataBankName")]
        public string DataBankName { get; set; }

        [XmlElement(ElementName = "AccessionNumberList")]
        public AccessionNumberList AccessionNumberList { get; set; }
    }
}