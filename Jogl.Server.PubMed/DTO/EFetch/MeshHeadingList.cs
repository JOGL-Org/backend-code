using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "MeshHeadingList")]
	public class MeshHeadingList
	{
		[XmlElement(ElementName = "MeshHeading")]
		public List<MeshHeading> MeshHeading { get; set; }
	}
}
