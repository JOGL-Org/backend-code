using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "PubmedArticleSet")]
	public class PubmedArticleSet
	{
		[XmlElement(ElementName = "PubmedArticle")]
		public List<PubmedArticle> PubmedArticles { get; set; }
	}
}
