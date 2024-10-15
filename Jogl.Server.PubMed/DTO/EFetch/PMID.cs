using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "PMID")]
	public class PMID
	{
		[XmlAttribute(AttributeName = "Version")]
		public string Version { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}