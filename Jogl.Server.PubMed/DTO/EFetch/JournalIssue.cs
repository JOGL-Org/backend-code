using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "JournalIssue")]
	public class JournalIssue
	{
		[XmlAttribute(AttributeName = "CitedMedium")]
		public string CitedMedium { get; set; }

		[XmlElement(ElementName = "Volume")]
		public string Volume { get; set; }

		[XmlElement(ElementName = "Issue")]
		public string Issue { get; set; }

		[XmlElement(ElementName = "PubDate")]
		public PubDate PubDate { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
