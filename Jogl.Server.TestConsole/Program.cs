using Jogl.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Business.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Jogl.Server.WhatsApp.Extensions;
using Jogl.Server.Search.Extensions;
using Jogl.Server.DB;
using Jogl.Server.Auth.Extensions;
using Jogl.Server.Cryptography.Extensions;
using Jogl.Server.Cryptography;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var host = Host.CreateDefaultBuilder()
           .ConfigureServices((context, services) =>
           {
               services.AddSingleton(config);
               services.AddWhatsApp(config);
               services.AddBusiness();
               services.AddSearch();
           })
           .Build();

var userRepository = host.Services.GetService<IUserRepository>();
var documentRepository = host.Services.GetService<IDocumentRepository>();
var paperRepository = host.Services.GetService<IPaperRepository>();
var resourceRepository = host.Services.GetService<IResourceRepository>();
var searchService = host.Services.GetService<Jogl.Server.Search.ISearchService>();

var users = userRepository.Query().ToList();
Console.WriteLine("users");
var documents = documentRepository.Query().ToList();
Console.WriteLine("documents loaded");
var papers = paperRepository.Query().ToList();
Console.WriteLine("papers loaded");
var resources = resourceRepository.Query().ToList();
Console.WriteLine("resources loaded");
await searchService.IndexUsersAsync(users, documents, papers, resources);

await host.StopAsync();
host.Dispose();

Console.WriteLine("Done");
Console.ReadLine();