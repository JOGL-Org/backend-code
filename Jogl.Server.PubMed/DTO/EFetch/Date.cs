using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
    [XmlRoot(ElementName = "DateCompleted")]
    public class Date
    {
        [XmlElement(ElementName = "Year")]
        public string Year { get; set; }

        [XmlElement(ElementName = "Month")]
        public string Month { get; set; }

        [XmlElement(ElementName = "Day")]
        public string Day { get; set; }

        [XmlIgnore]
        public DateTime? Value
        {
            get
            {
                DateTime d;
                if (!DateTime.TryParse($"{Day}/{Month}/{Year}", out d))
                    return null;

                return d;
            }
        }
    }
}
