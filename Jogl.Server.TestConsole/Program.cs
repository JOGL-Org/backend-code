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

var paperRepo = new PaperRepository(config);
var needRepo = new NeedRepository(config);
var feedRepo = new FeedRepository(config);

var papers = paperRepo.List(p => !p.Deleted);
foreach (var pap in papers)
{
    var i = 0;
    foreach (var feedId in pap.FeedIds ?? new List<string>())
    {
        if (i++ == 0)
        {
            pap.FeedId = feedId;
            pap.FeedIds = new List<string>();
            pap.DefaultVisibility = Jogl.Server.Data.FeedEntityVisibility.Comment;
            await paperRepo.UpdateAsync(pap);
        }
        else
        {
            var id = await feedRepo.CreateAsync(new Jogl.Server.Data.Feed
            {
                CreatedByUserId = pap.CreatedByUserId,
                CreatedUTC = pap.CreatedUTC,
                Type = Jogl.Server.Data.FeedType.Paper,
            });

            await paperRepo.CreateAsync(new Jogl.Server.Data.Paper
            {
                Id = ObjectId.Parse(id),
                FeedId = feedId,
                Authors = pap.Authors,
                CreatedByUserId = pap.CreatedByUserId,
                CreatedUTC = pap.CreatedUTC,
                ExternalId = pap.ExternalId,
                ExternalSystem = pap.ExternalSystem,
                Journal = pap.Journal,
                DefaultVisibility = Jogl.Server.Data.FeedEntityVisibility.Comment,
                OpenAccessPdfUrl = pap.OpenAccessPdfUrl,
                PublicationDate = pap.PublicationDate,
                Status = pap.Status,
                Summary = pap.Summary,
                TagData = pap.TagData,
                Tags = pap.Tags,
                Title = pap.Title,
                Type = pap.Type,
                UpdatedByUserId = pap.UpdatedByUserId,
                UpdatedUTC = pap.UpdatedUTC,
                UserIds = pap.UserIds,
            });
        }
    }
}
var needs = needRepo.List(n => !n.Deleted);
foreach (var n in needs)
{
    n.CommunityEntityVisibility = new List<Jogl.Server.Data.FeedEntityCommunityEntityVisibility> { new Jogl.Server.Data.FeedEntityCommunityEntityVisibility
    {
            CommunityEntityId = n.EntityId,
            Visibility = Jogl.Server.Data.FeedEntityVisibility.Comment,
    }};

    await needRepo.UpdateAsync(n);
}

Console.ReadLine();
