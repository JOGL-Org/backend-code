using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "PubMedPubDate")]
	public class PubMedPubDate
	{
		[XmlElement(ElementName = "Year")]
		public string Year { get; set; }

		[XmlElement(ElementName = "Month")]
		public string Month { get; set; }

		[XmlElement(ElementName = "Day")]
		public string Day { get; set; }

		[XmlElement(ElementName = "Hour")]
		public string Hour { get; set; }

		[XmlElement(ElementName = "Minute")]
		public string Minute { get; set; }

		[XmlElement(ElementName = "Second")]
		public string Second { get; set; }

		[XmlAttribute(AttributeName = "PubStatus")]
		public string PubStatus { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
