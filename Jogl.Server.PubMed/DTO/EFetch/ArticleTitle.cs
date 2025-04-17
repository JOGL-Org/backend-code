using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "ArticleTitle")]
	public class ArticleTitle
	{
		[XmlText]
		public string Text { get; set; }
	}
}
