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

Console.WriteLine("Done");
Console.ReadLine();