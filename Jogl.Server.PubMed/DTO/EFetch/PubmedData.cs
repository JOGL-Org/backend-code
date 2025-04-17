using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "PubmedData")]
	public class PubmedData
	{
		[XmlElement(ElementName = "History")]
		public History History { get; set; }

		[XmlElement(ElementName = "PublicationStatus")]
		public string PublicationStatus { get; set; }

		[XmlElement(ElementName = "ArticleIdList")]
		public ArticleIdList ArticleIdList { get; set; }

		[XmlElement(ElementName = "ReferenceList")]
		public ReferenceList ReferenceList { get; set; }
	}
}
