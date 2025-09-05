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
await app.DeleteAsync("68babc169c330887f3ad983b");
await app.DeleteAsync("68babe2cde137e0e9c565902");
await app.DeleteAsync("68b001ea3f34ae148de73453");
await app.DeleteAsync("68b001f03f34ae148de73454");
await app.DeleteAsync("68b997289c330887f3ad9808");
await app.DeleteAsync("68b9995c9c330887f3ad9824");

await host.StopAsync();
host.Dispose();

Console.WriteLine("Done");
Console.ReadLine();