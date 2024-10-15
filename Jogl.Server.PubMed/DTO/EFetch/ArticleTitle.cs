using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "ArticleTitle")]
	public class ArticleTitle
	{
		[XmlText]
		public string Text { get; set; }
	}
}
