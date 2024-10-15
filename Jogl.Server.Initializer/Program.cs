using Jogl.Server.Data;
using System.Xml;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Jogl.Server.DB;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

//load MeSH tags
var doc = new XmlDocument();
doc.LoadXml(File.ReadAllText("MeSH-desc.xml"));
var tags = new List<Tag>();
var tagRepository = new TagRepository(config);


foreach (XmlNode record in doc.SelectNodes("DescriptorRecordSet/DescriptorRecord"))
{
    var name = record.SelectSingleNode("DescriptorName/String").InnerText;
    var ids = new List<string>();
    foreach (XmlNode treeNumber in record.SelectNodes("TreeNumberList/TreeNumber"))
    {
        ids.Add(treeNumber.InnerText);
    }

    var desc = record.SelectSingleNode("ConceptList/Concept[@PreferredConceptYN=\"Y\"]/ScopeNote")?.InnerText;

    tags.Add(new Tag { Text = name, LinkedIds = ids, Description = desc, Source = "MeSH" });
}

await tagRepository.CreateAsync(tags);

var skills = new List<TextValue>();
var skillRepository = new SkillRepository(config);
foreach (var line in File.ReadAllLines("LinkedIn-Skills.csv"))
{
    var splitLine = line.Split(',');
    skills.Add(new TextValue { Value = splitLine[1] });
}

await skillRepository.CreateAsync(skills);

//foreach (var tag in tags)
//{
//    await skillRepository.CreateAsync(tag);
//}



