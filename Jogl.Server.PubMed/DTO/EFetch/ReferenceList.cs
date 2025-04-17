using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "ReferenceList")]
	public class ReferenceList
	{
		[XmlElement(ElementName = "Reference")]
		public List<Reference> Reference { get; set; }

		[XmlElement(ElementName = "Title")]
		public string Title { get; set; }
	}
}
