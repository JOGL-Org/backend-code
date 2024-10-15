using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "CommentsCorrectionsList")]
	public class CommentsCorrectionsList
	{
		[XmlElement(ElementName = "CommentsCorrections")]
		public CommentsCorrections CommentsCorrections { get; set; }
	}
}
