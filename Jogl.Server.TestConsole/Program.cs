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
var membershipRepo = new MembershipRepository(config);


var ufrs = userFeedRecordRepository.Query(ufr => true).ToList();
var memberships = membershipRepo.Query(m => m.CommunityEntityType == Jogl.Server.Data.CommunityEntityType.Channel).ToList();
foreach (var membership in memberships)
{
    if (ufrs.SingleOrDefault(ufr => ufr.FeedId == membership.CommunityEntityId && ufr.UserId == membership.UserId)?.FollowedUTC == null)
    {
        await userFeedRecordRepository.SetFeedFollowedAsync(membership.UserId, membership.CommunityEntityId, DateTime.UtcNow);
    }
}

Console.WriteLine("Done");
Console.ReadLine();