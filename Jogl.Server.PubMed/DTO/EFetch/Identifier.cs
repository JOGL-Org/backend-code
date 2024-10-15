using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "Identifier")]
	public class Identifier
	{
		[XmlAttribute(AttributeName = "Source")]
		public string Source { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
