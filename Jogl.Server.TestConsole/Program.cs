using Jogl.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Business.Extensions;
using Microsoft.Extensions.Hosting;
using Jogl.Server.Business;
using Microsoft.Extensions.DependencyInjection;

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
               services.AddBusiness();
           })
           .Build();

// Resolve and use services
var app = host.Services.GetRequiredService<IEventService>();
await app.DeleteAsync("682b500d6363951c53aac2b2");

await host.StopAsync();
host.Dispose();

Console.WriteLine("Done");
Console.ReadLine();