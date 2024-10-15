using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "Keyword")]
	public class Keyword
	{
		[XmlAttribute(AttributeName = "MajorTopicYN")]
		public string MajorTopicYN { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
