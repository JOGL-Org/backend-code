using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "AbstractText")]
	public class AbstractText
	{
		[XmlAttribute(AttributeName = "Label")]
		public string Label { get; set; }

		[XmlAttribute(AttributeName = "NlmCategory")]
		public string NlmCategory { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
