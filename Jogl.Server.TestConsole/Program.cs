using Jogl.Server.Configuration;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var ceRepo = new ContentEntityRepository(config);
var cesToDelete = ceRepo.List(ce => !string.IsNullOrEmpty(ce.ExternalID));
await ceRepo.DeleteAsync(cesToDelete);

Console.ReadLine();
