using Jogl.Server.Configuration;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("Environment")}.json")
    .AddKeyVault()
    .Build();

var membershipRepo = new MembershipRepository(config);
var channelMembershipsWithOwner = membershipRepo.Query(m => m.CommunityEntityType == Jogl.Server.Data.CommunityEntityType.Channel && m.AccessLevel == Jogl.Server.Data.AccessLevel.Owner).ToList();
foreach (var channelMembership in channelMembershipsWithOwner)
{
    channelMembership.AccessLevel = Jogl.Server.Data.AccessLevel.Admin;
    await membershipRepo.UpdateAsync(channelMembership);
}

var ucerRepo = new UserContentEntityRecordRepository(config);
var ufrRepo = new UserFeedRecordRepository(config);
var ceRepo = new ContentEntityRepository(config);

var ucers = ucerRepo.List(ucer => true);
var ufrs = ufrRepo.List(ufr => true);
var ces = ceRepo.List(ufr => true);
foreach (var ucer in ucers)
{
    if (ucer.LastWriteUTC.HasValue)
        await ucerRepo.SetContentEntityWrittenAsync(ucer.UserId, ucer.FeedId, ucer.ContentEntityId, ucer.LastWriteUTC.Value);

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

foreach (var ce in ces)
{
    if (ce.LastActivityUTC == null)
    {
        ce.LastActivityUTC = ce.CreatedUTC;
        await ceRepo.UpdateAsync(ce);
    }
}

var docRepo = new DocumentRepository(config);
var evRepo = new EventRepository(config);
var paperRepo = new PaperRepository(config);
var needRepo = new NeedRepository(config);

var documents = docRepo.Query().ToList();
var events = evRepo.Query().ToList();
var papers = paperRepo.Query().ToList();
var needs = needRepo.Query().ToList();

foreach (var doc in documents)
{
    if (doc.UpdatedUTC == null)
    {
        doc.UpdatedUTC = doc.CreatedUTC;
        await docRepo.UpdateAsync(doc);
    }
}

foreach (var ev in events)
{
    if (ev.UpdatedUTC == null)
    {
        ev.UpdatedUTC = ev.CreatedUTC;
        await evRepo.UpdateAsync(ev);
    }
}

foreach (var paper in papers)
{
    if (paper.UpdatedUTC == null)
    {
        paper.UpdatedUTC = paper.CreatedUTC;
        await paperRepo.UpdateAsync(paper);
    }
}

foreach (var need in needs)
{
    if (need.UpdatedUTC == null)
    {
        need.UpdatedUTC = need.CreatedUTC;
        await needRepo.UpdateAsync(need);
    }
}

Console.WriteLine("Done");
Console.ReadLine();