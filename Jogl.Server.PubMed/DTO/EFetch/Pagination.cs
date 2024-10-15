using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "Pagination")]
	public class Pagination
	{
		[XmlElement(ElementName = "StartPage")]
		public string StartPage { get; set; }

		[XmlElement(ElementName = "MedlinePgn")]
		public string MedlinePgn { get; set; }

		[XmlElement(ElementName = "EndPage")]
		public string EndPage { get; set; }
	}
}
