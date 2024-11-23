using System.Xml.Serialization;

namespace Jogl.Server.PubMed.DTO
{
	[XmlRoot(ElementName = "MedlineCitation")]
	public class MedlineCitation
	{
		[XmlAttribute(AttributeName = "Owner")]
		public string Owner { get; set; }

		[XmlAttribute(AttributeName = "Status")]
		public string Status { get; set; }

		[XmlAttribute(AttributeName = "IndexingMethod")]
		public string IndexingMethod { get; set; }

		[XmlAttribute(AttributeName = "VersionID")]
		public string VersionID { get; set; }

		[XmlAttribute(AttributeName = "VersionDate")]
		public string VersionDate { get; set; }

		[XmlElement(ElementName = "PMID")]
		public PMID PMID { get; set; }

		[XmlElement(ElementName = "Created")]
		public Date DateCreated { get; set; }

		[XmlElement(ElementName = "DateCompleted")]
		public Date DateCompleted { get; set; }

		[XmlElement(ElementName = "DateRevised")]
		public Date DateRevised { get; set; }

		[XmlElement(ElementName = "Article")]
		public Article Article { get; set; }

		[XmlElement(ElementName = "MedlineJournalInfo")]
		public MedlineJournalInfo MedlineJournalInfo { get; set; }

		[XmlElement(ElementName = "ChemicalList")]
		public ChemicalList ChemicalList { get; set; }

		[XmlElement(ElementName = "SupplMeshList")]
		public SupplMeshList SupplMeshList { get; set; }

		[XmlElement(ElementName = "CitationSubset")]
		public string CitationSubset { get; set; }

		[XmlElement(ElementName = "CommentsCorrectionsList")]
		public CommentsCorrectionsList CommentsCorrectionsList { get; set; }

		[XmlElement(ElementName = "GeneSymbolList")]
		public GeneSymbolList GeneSymbolList { get; set; }

		[XmlElement(ElementName = "MeshHeadingList")]
		public MeshHeadingList MeshHeadingList { get; set; }

		[XmlElement(ElementName = "NumberOfReferences")]
		public string NumberOfReferences { get; set; }

		[XmlElement(ElementName = "PersonalNameSubjectList")]
		public PersonalNameSubjectList PersonalNameSubjectList { get; set; }

		[XmlElement(ElementName = "OtherID")]
		public OtherID OtherID { get; set; }

		[XmlElement(ElementName = "OtherAbstract")]
		public OtherAbstract OtherAbstract { get; set; }

		[XmlElement(ElementName = "KeywordList")]
		public KeywordList KeywordList { get; set; }

		[XmlElement(ElementName = "CoiStatement")]
		public string CoiStatement { get; set; }

		[XmlElement(ElementName = "SpaceFlightMission")]
		public string SpaceFlightMission { get; set; }

		[XmlElement(ElementName = "InvestigatorList")]
		public InvestigatorList InvestigatorList { get; set; }

		[XmlElement(ElementName = "GeneralNote")]
		public GeneralNote GeneralNote { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}
