using Jogl.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Business.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Jogl.Server.WhatsApp.Extensions;
using Jogl.Server.WhatsApp;
using Jogl.Server.Business;

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
           })
           .Build();

var userService = host.Services.GetService<IUserService>();
var users = userService.List();
foreach (var user in users)
{
    if (user.Onboarding == true)
        continue;

    user.Onboarding = true;
}

//v/ar whatsappService = host.Services.GetRequiredService<IWhatsAppService>();
//await whatsappService.SendMessageAsync("447504849281", "kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot  kokot");

//var app = host.Services.GetRequiredService<IEventService>();
//await app.DeleteAsync("68babc169c330887f3ad983b");
//await app.DeleteAsync("68babe2cde137e0e9c565902");
//await app.DeleteAsync("68b001ea3f34ae148de73453");
//await app.DeleteAsync("68b001f03f34ae148de73454");
//await app.DeleteAsync("68b997289c330887f3ad9808");
//await app.DeleteAsync("68b9995c9c330887f3ad9824");

await host.StopAsync();
host.Dispose();

Console.WriteLine("Done");
Console.ReadLine();