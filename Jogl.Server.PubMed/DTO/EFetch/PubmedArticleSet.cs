using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "PubmedArticleSet")]
	public class PubmedArticleSet
	{
		[XmlElement(ElementName = "PubmedArticle")]
		public List<PubmedArticle> PubmedArticles { get; set; }
	}
}
