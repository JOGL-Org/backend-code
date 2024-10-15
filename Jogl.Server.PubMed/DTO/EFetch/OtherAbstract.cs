using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "OtherAbstract")]
	public class OtherAbstract
	{
        [XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }

		[XmlElement(ElementName = "AbstractText")]
		public List<AbstractText> AbstractText { get; set; }

		[XmlElement(ElementName = "CopyrightInformation")]
		public string CopyrightInformation { get; set; }

        [XmlText]
		public string Text { get; set; }
	}
}
