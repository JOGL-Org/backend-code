using Amazon.Runtime.Internal;
using Jogl.Server.Configuration;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var userFeedRecordRepository = new UserFeedRecordRepository(config);
var userContentEntityRecordRepository = new UserContentEntityRecordRepository(config);
var mentionRepository = new MentionRepository(config);
var contentEntityRepository = new ContentEntityRepository(config);
var commentRepository = new CommentRepository(config);
var membershipRepo = new MembershipRepository(config);
var feedRepository = new FeedRepository(config);
var documentRepository = new DocumentRepository(config);

var feeds = feedRepository.List(f => true);
foreach (var ufr in userFeedRecordRepository.List(u => !u.Deleted))
{
    var feed = feeds.FirstOrDefault(f => f.Id.ToString() == ufr.FeedId);
    if (feed == null)
    {
        await userFeedRecordRepository.DeleteAsync(ufr);
        continue;
    }
    else
    {



    }

    if (feed.Deleted)
    {
        await userFeedRecordRepository.DeleteAsync(ufr);
        continue;
    }
}

Console.WriteLine("Done");
Console.ReadLine();