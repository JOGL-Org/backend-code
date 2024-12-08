using Jogl.Server.Configuration;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var ucerRepo = new UserContentEntityRecordRepository(config);
var ufrRepo = new UserFeedRecordRepository(config);

var ucers = ucerRepo.List(ucer=>true);
var ufrs = ufrRepo.List(ufr=>true);
foreach (var ucer in ucers)
{
    if(ucer.LastWriteUTC.HasValue)
        await ucerRepo.SetContentEntityWrittenAsync(ucer.UserId,ucer.FeedId, ucer.ContentEntityId, ucer.LastWriteUTC.Value);

    if (ucer.LastMentionUTC.HasValue)
        await ucerRepo.SetContentEntityMentionAsync(ucer.UserId, ucer.FeedId, ucer.ContentEntityId, ucer.LastMentionUTC.Value);
}

foreach (var ufr in ufrs)
{
    if (ufr.LastWriteUTC.HasValue)
        await ufrRepo.SetFeedWrittenAsync(ufr.UserId, ufr.FeedId, ufr.LastWriteUTC.Value);

    if (ufr.LastMentionUTC.HasValue)
        await ufrRepo.SetFeedMentionAsync(ufr.UserId, ufr.FeedId, ufr.LastMentionUTC.Value);
}

Console.WriteLine("Done");
Console.ReadLine();