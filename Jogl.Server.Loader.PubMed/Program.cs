//initial load

using Jogl.Server.Loader.PubMed.DTO;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System.Dynamic;
using System.Xml.Linq;

var doc = XDocument.Load("pubmed-tags-2024.xml");
////string jsonText = JsonConvert.SerializeXNode(doc);
////dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);

// XmlSerializer serializer = new XmlSerializer(typeof(DescriptorRecordSet));
//using (StringReader reader = new StringReader("pubmed-tags-2024.xml"))
//{
//    var data = (DescriptorRecordSet)serializer.Deserialize(reader);
//}


var drs = doc.Root.Elements("DescriptorRecord");
var dic = new Dictionary<string, List<string>>();
foreach (var dr in drs)
{
    if (dr.Element("TreeNumberList") == null)
        continue;

    dic.Add(dr.Element("DescriptorName").Value, dr.Element("TreeNumberList").Elements("TreeNumber").Select(tn=>tn.Value).ToList());
}

File.WriteAllLines("pubmed_categories.txt", dic.OrderBy(dr => dr.Value.FirstOrDefault()).Select(dr => $"{dr.Key}|{string.Join(",", dr.Value)}"));

