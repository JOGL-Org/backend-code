using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "QualifierName")]
	public class QualifierName
	{
		[XmlAttribute(AttributeName = "UI")]
		public string UI { get; set; }

		[XmlAttribute(AttributeName = "MajorTopicYN")]
		public string MajorTopicYN { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
