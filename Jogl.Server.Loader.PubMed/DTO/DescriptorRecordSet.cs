using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Jogl.Server.Loader.PubMed.DTO
{
    [XmlRoot(ElementName = "DescriptorRecordSet")]
    public class DescriptorRecordSet
    {
        [XmlElement(ElementName = "DescriptorRecord")]
        public List<DescriptorRecord> DescriptorRecord { get; set; }
    }
}
