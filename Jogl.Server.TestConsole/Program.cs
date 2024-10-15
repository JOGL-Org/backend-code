using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .Build();

var paperRepository = new PaperRepository(config);
var papers = paperRepository.List(a => !a.Deleted && !string.IsNullOrEmpty(a.Journal));

foreach (var paper in papers)
{
    Console.WriteLine(paper.Journal);
    try
    {
        var journalData = JsonSerializer.Deserialize<JournalData>(paper.Journal);
        paper.Journal = journalData.journal;
        paper.OpenAccessPdfUrl = journalData.open_access_pdf;

        paperRepository.UpdateAsync(paper).Wait();
    }
    catch (Exception)
    {
        continue;
    }
}
Console.ReadLine();

class JournalData
{
    public string journal { get; set; }
    public string open_access_pdf { get; set; }
}