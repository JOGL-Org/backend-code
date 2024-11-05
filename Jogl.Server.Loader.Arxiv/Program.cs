using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Jogl.Server.Configuration;
using Jogl.Server.Data;
using System.Globalization;
using MoreLinq;
using Jogl.Server.Arxiv;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var publicationRepository = new PublicationRepository(config);

////initial load
//var i = 0;
//foreach (var lineBatch in File.ReadLines("arxiv-metadata-oai-snapshot.json").Batch(10000))
//{
//    var publicationBatch = new List<Publication>();

//    foreach (var line in lineBatch)
//    {
//        var ap = System.Text.Json.JsonSerializer.Deserialize<ArxivPublication>(line);
//        DateTime published;
//        var publishDateParsed = DateTime.TryParseExact(ap.Versions.FirstOrDefault()?.Created, "ddd, d MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out published);

//        var id = ap.Id.Replace("oai:arXiv.org:", string.Empty);

//        var publication = new Publication
//        {
//            Authors = ap.AuthorsParsed.Select(a => string.Join(" ", a.Reverse<string>().Where(str => !string.IsNullOrEmpty(str)))).ToList(),
//            CreatedUTC = DateTime.UtcNow,
//            DOI = ap.Doi,
//            Journal = ap.JournalRef,
//            LicenseURL = ap.License,
//            Published = publishDateParsed ? published : null,
//            ExternalID = id,
//            Submitter = ap.Submitter,
//            ExternalSystem = "ARXIV",
//            ExternalURL = $"https://arxiv.org/abs/{id}",
//            ExternalFileURL = $"https://arxiv.org/pdf/{id}",
//            Summary = ap.Abstract.Trim(),
//            Tags = ap.Categories.Split(' ').ToList(),
//            Title = ap.Title
//        };

//        publicationBatch.Add(publication);
//    }

//    await publicationRepository.CreateBulkAsync(publicationBatch);
//    Console.WriteLine(++i);
//}

//load new papers from arxiv
var arxivFacade = new ArxivFacade(config, null);
var entries = await arxivFacade.ListNewPapersAsync(DateTime.UtcNow.AddDays(-2));

//transform to publication object
var publications = entries.Select(entry =>
{
    var id = entry.Id.Replace("http://arxiv.org/abs/", string.Empty);
    var idWithoutVersion = id.Substring(0, id.IndexOf("v"));

    return new Publication
    {
        Authors = entry.Author.Select(a => a.Name).ToList(),
        CreatedUTC = DateTime.UtcNow,
        DOI = entry.Doi?.Text,
        Journal = entry.JournalRef?.Text,
        //LicenseURL = entry.
        Published = entry.Published,
        ExternalID = idWithoutVersion,
        //Submitter =entry.
        ExternalSystem = "ARXIV",
        ExternalURL = $"https://arxiv.org/abs/{idWithoutVersion}",
        ExternalFileURL = $"https://arxiv.org/pdf/{idWithoutVersion}",
        Summary = entry.Summary,
        Tags = entry.Category.Select(c => c.Term).ToList(),
        Title = entry.Title
    };
});

//store papers
await Parallel.ForEachAsync(publications, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (publication, cancellationToken) =>
{
    await publicationRepository.UpsertAsync(publication, p => p.ExternalID);
    Console.WriteLine(publication.Published);
});

Console.ReadLine();

public class ArxivPublication
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("submitter")]
    public string Submitter { get; set; }

    [JsonPropertyName("authors")]
    public string Authors { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("comments")]
    public string Comments { get; set; }

    [JsonPropertyName("journal-ref")]
    public string JournalRef { get; set; }

    [JsonPropertyName("doi")]
    public string Doi { get; set; }

    [JsonPropertyName("report-no")]
    public string ReportNo { get; set; }

    [JsonPropertyName("categories")]
    public string Categories { get; set; }

    [JsonPropertyName("license")]
    public string License { get; set; }

    [JsonPropertyName("abstract")]
    public string Abstract { get; set; }

    [JsonPropertyName("versions")]
    public List<ArxivPublicationVersion> Versions { get; set; }

    [JsonPropertyName("update_date")]
    public string UpdateDate { get; set; }

    [JsonPropertyName("authors_parsed")]
    public List<List<string>> AuthorsParsed { get; set; }
}

public class ArxivPublicationVersion
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }
}

