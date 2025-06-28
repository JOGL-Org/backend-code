using Jogl.Server.AI;
using Jogl.Server.Configuration;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Lix;
using Jogl.Server.SerpAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using DnsClient.Internal;
using Jogl.Server.Business;
using Jogl.Server.Storage;
using Jogl.Server.Documents;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Nodes;
using Jogl.Server.OpenAlex;
using Jogl.Server.SemanticScholar;
using Jogl.Server.Orcid;
using MongoDB.Bson;
using Jogl.Server.PubMed.DTO.EFetch;
using System.Text;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Drawing.Text;
using Jogl.Server.Events;
using Jogl.Server.URL;
using Google.Apis.Calendar.v3;
using Jogl.Server.Lix.DTO;
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
var membershipRepository = new MembershipRepository(config);
var invitationRepository = new InvitationRepository(config);
var feedRepository = new FeedRepository(config);
var eventRepository = new EventRepository(config);
var eventAttendanceRepository = new EventAttendanceRepository(config);
var documentRepository = new DocumentRepository(config);
var workspaceRepository = new WorkspaceRepository(config);
var userRepository = new UserRepository(config);
var paperRepository = new PaperRepository(config);
var resourceRepository = new ResourceRepository(config);
var channelRepository = new ChannelRepository(config);

await membershipRepository.CreateAsync(new Membership
{
    CommunityEntityId = "674f3ceda442a3424df80b05",
    CommunityEntityType = CommunityEntityType.Node,
    UserId = "650804ea64460e828ac486bf",
    CreatedByUserId = "650804ea64460e828ac486bf",
    CreatedUTC = DateTime.UtcNow,
    AccessLevel = AccessLevel.Owner,
});

var existingChannels = channelRepository.Query(c => !string.IsNullOrEmpty(c.Key)).ToList();
foreach (var user in userRepository.Query().ToList())
{
    if (user.CreatedUTC>)
        continue;

    await userRepository.SetStatusAsync(user.Id.ToString(), UserStatus.Verified);
    continue;

    var existingChannel = existingChannels.SingleOrDefault(c => c.CommunityEntityId == user.Id.ToString());
    if (existingChannel != null)
        continue;

    var channelId = await channelRepository.CreateAsync(new Channel
    {
        Title = "Search Agent",
        Description = "An AI-powered conversational agent that helps you search our database of experts",
        Key = "USER_SEARCH",
        CommunityEntityId = user.Id.ToString(),
        CreatedByUserId = user.Id.ToString(),
        CreatedUTC = DateTime.UtcNow,
    });

    await feedRepository.CreateAsync(new Feed
    {
        CreatedUTC = DateTime.UtcNow,
        Id = ObjectId.Parse(channelId),
        Type = FeedType.Channel,
    });

    var membershipId = await membershipRepository.CreateAsync(new Membership
    {
        AccessLevel = AccessLevel.Member,
        CommunityEntityId = channelId,
        UserId = user.Id.ToString(),
        CreatedByUserId = user.Id.ToString(),
        CreatedUTC = DateTime.UtcNow,
    });
}



Console.WriteLine("Done");
Console.ReadLine();