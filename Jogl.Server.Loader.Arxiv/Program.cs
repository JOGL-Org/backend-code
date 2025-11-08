using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Configuration;
using MoreLinq;
using Azure.Identity;
using Azure.Storage.Blobs;
using Jogl.Server.Cryptography;
using System.Text;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var publicationRepository = new PublicationRepository(config);

var serviceClient = new BlobServiceClient(new Uri($"https://jogldatastore.blob.core.windows.net"), new DefaultAzureCredential());
var blobContainer = serviceClient.GetBlobContainerClient("publications");
await blobContainer.CreateIfNotExistsAsync();

var hashService = new CryptographyService();
var i = 0;
foreach (var lineBatch in File.ReadLines("arxiv-metadata-oai-snapshot.json").Batch(10000))
{
    Console.WriteLine(i++);
    await Parallel.ForEachAsync(lineBatch, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (line, cancellationToken) =>
    {
        var id = hashService.ComputeHash(line);
        var folder = "arxiv";

        string blobPath = string.IsNullOrEmpty(folder) ? id : $"{folder.TrimEnd('/')}/{id}.json";
        var client = blobContainer.GetBlobClient(blobPath);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(line));
        await client.UploadAsync(stream, overwrite: true);
    });
}

//public class ArxivPublication
//{
//    [JsonPropertyName("id")]
//    public string Id { get; set; }

//    [JsonPropertyName("submitter")]
//    public string Submitter { get; set; }

//    [JsonPropertyName("authors")]
//    public string Authors { get; set; }

//    [JsonPropertyName("title")]
//    public string Title { get; set; }

//    [JsonPropertyName("comments")]
//    public string Comments { get; set; }

//    [JsonPropertyName("journal-ref")]
//    public string JournalRef { get; set; }

//    [JsonPropertyName("doi")]
//    public string Doi { get; set; }

//    [JsonPropertyName("report-no")]
//    public string ReportNo { get; set; }

//    [JsonPropertyName("categories")]
//    public string Categories { get; set; }

//    [JsonPropertyName("license")]
//    public string License { get; set; }

//    [JsonPropertyName("abstract")]
//    public string Abstract { get; set; }

//    [JsonPropertyName("versions")]
//    public List<ArxivPublicationVersion> Versions { get; set; }

//    [JsonPropertyName("update_date")]
//    public string UpdateDate { get; set; }

//    [JsonPropertyName("authors_parsed")]
//    public List<List<string>> AuthorsParsed { get; set; }
//}

//public class ArxivPublicationVersion
//{
//    [JsonPropertyName("version")]
//    public string Version { get; set; }

//    [JsonPropertyName("created")]
//    public string Created { get; set; }
//}