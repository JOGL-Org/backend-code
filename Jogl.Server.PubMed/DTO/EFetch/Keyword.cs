using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
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
