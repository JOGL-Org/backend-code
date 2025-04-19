using Jogl.Server.Configuration;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
var documentRepository = new DocumentRepository(config);
var workspaceRepository = new WorkspaceRepository(config);
var userRepository = new UserRepository(config);
var paperRepository = new PaperRepository(config);
var resourceRepository = new ResourceRepository(config);

var json = File.ReadAllText("C:\\code\\Seed_data_synbio.json");
var data = JsonSerializer.Deserialize<List<User>>(json);
foreach (var user in data)
{
    try
    {
        user.Status = UserStatus.Verified;

        var id = await userRepository.CreateAsync(user);
        foreach (var paper in user.Papers)
        {
            paper.FeedId = id;
            paper.DefaultVisibility = FeedEntityVisibility.View;
            await paperRepository.CreateAsync(paper);
        }

        foreach (var doc in user.Documents)
        {
            doc.FeedId = id;
            doc.Type = DocumentType.JoglDoc;
            doc.DefaultVisibility = FeedEntityVisibility.View;
            await documentRepository.CreateAsync(doc);
        }

        foreach (var res in user.Resources)
        {
            res.EntityId = id;
            res.DefaultVisibility = FeedEntityVisibility.View;
            await resourceRepository.CreateAsync(res);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}

Console.WriteLine("Done");
Console.ReadLine();