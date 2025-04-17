using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO.EFetch
{
	[XmlRoot(ElementName = "MeshHeading")]
	public class MeshHeading
	{
		[XmlElement(ElementName = "DescriptorName")]
		public DescriptorName DescriptorName { get; set; }

		[XmlElement(ElementName = "QualifierName")]
		public List<QualifierName> QualifierName { get; set; }
	}
}
