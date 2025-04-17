using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "KeywordList")]
	public class KeywordList
	{
		[XmlAttribute(AttributeName = "Owner")]
		public string Owner { get; set; }

		[XmlElement(ElementName = "Keyword")]
		public List<Keyword> Keyword { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
