using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "DescriptorName")]
	public class DescriptorName
	{
		[XmlAttribute(AttributeName = "UI")]
		public string UI { get; set; }

		[XmlAttribute(AttributeName = "MajorTopicYN")]
		public string MajorTopicYN { get; set; }

		[XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
