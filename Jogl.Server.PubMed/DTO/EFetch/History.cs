using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "History")]
	public class History
	{
		[XmlElement(ElementName = "PubMedPubDate")]
		public List<PubMedPubDate> PubMedPubDate { get; set; }
	}
}
