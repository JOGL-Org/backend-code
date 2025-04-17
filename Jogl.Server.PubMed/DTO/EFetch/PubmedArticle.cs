using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "PubmedArticle")]
	public class PubmedArticle
	{
		[XmlElement(ElementName = "MedlineCitation")]
		public MedlineCitation MedlineCitation { get; set; }

		[XmlElement(ElementName = "PubmedData")]
		public PubmedData PubmedData { get; set; }
	}
}
