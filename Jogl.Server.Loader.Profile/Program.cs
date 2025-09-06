using Jogl.Server.Configuration;
using Jogl.Server.Data;
using Microsoft.Extensions.Configuration;
using Jogl.Server.Business;
using System.Text.Json;
using Jogl.Server.PubMed.DTO.EFetch;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Jogl.Server.Business.Extensions;
using User = Jogl.Server.Loader.Profile.DTO.User;

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
var userService = host.Services.GetRequiredService<IUserService>();
var membershipService = host.Services.GetRequiredService<IMembershipService>();

var existingUsers = userService.List(null, null, 1, int.MaxValue, Jogl.Server.Data.Util.SortKey.CreatedDate, false);
var pasteurNodeId = "684bf7e24b1507a00823d6a3";


foreach (var file in Directory.GetFiles("../../../pasteur/"))
{
    try
    {
        var jsonString = File.ReadAllText(file);
        var importUser = JsonSerializer.Deserialize<User>(jsonString);
        if (string.IsNullOrEmpty(importUser.Email))
        {
            Console.WriteLine($"{importUser.FirstName} {importUser.LastName} has no email");
            continue;
        }

        var user = existingUsers.Items.SingleOrDefault(u => u.FirstName == importUser.FirstName && u.LastName == importUser.LastName);
        if (user == null)
        {
            user = new Jogl.Server.Data.User
            {
                Bio = importUser.Bio,
                ShortBio = importUser.Headline,
                CreatedUTC = DateTime.UtcNow,
                Current = importUser.LatestActivities,
                FirstName = importUser.FirstName,
                LastName = importUser.LastName,
                Email = importUser.Email,
                Experience = importUser.Experience?.Select(e => new UserExperience
                {
                    Company = e.Company,
                    Description = e.Description,
                    Position = e.Position,
                    DateFrom = e.DateFrom?.ToString(),
                    DateTo = e.DateTo?.ToString(),
                    Current = e.Current,
                })?.ToList(),
                Education = importUser.Education?.Select(e => new UserEducation
                {
                    Program = e.Program,
                    Description = e.Description,
                    School = e.School,
                    DateFrom = e.DateFrom?.ToString(),
                    DateTo = e.DateTo?.ToString(),
                    Current = e.Current,
                })?.ToList(),
                Skills = importUser.Skills?.Select(s => s.SkillName)?.ToList(),
                Status = UserStatus.Verified
            };

            var userId = await userService.CreateAsync(user);
            await membershipService.CreateAsync(new Membership
            {
                CommunityEntityId = pasteurNodeId,
                CommunityEntityType = CommunityEntityType.Node,
                CreatedByUserId = userId,
                CreatedUTC = DateTime.UtcNow,
                UserId = userId
            });
        }
        else
        {
            //await membershipService.AddMembersAsync([new Membership
            //{
            //    CommunityEntityId = pasteurNodeId,
            //    CommunityEntityType = CommunityEntityType.Node,
            //    CreatedByUserId = user.Id.ToString(),
            //    CreatedUTC = DateTime.UtcNow,
            //    UserId = user.Id.ToString()
            //}]);
        }



        //// repositories
        //var repos = json["repolist"]?.AsArray();
        //Console.WriteLine($"{repos?.Count} repositories");
        //foreach (JsonNode repo in repos ?? new JsonArray())
        //{
        //    var title = repo.AsObject().ContainsKey("name") ? repo["name"]?.GetValue<string>() : null;
        //    var description = repo.AsObject().ContainsKey("description") ? repo["description"]?.GetValue<string>() : null;
        //    var abst = repo.AsObject().ContainsKey("abstract") ? repo["abstract"]?.GetValue<string>() : null;
        //    var keywords = repo.AsObject().ContainsKey("keywords") ? (repo["keywords"] as JsonArray).Select(node => node.GetValue<string>()).ToList() : null;
        //    var readme = repo.AsObject().ContainsKey("readme") ? repo["readme"]?.GetValue<string>() : null;
        //    var homepage = repo.AsObject().ContainsKey("homepage") ? repo["homepage"]?.GetValue<string>() : null;

        //    if (string.IsNullOrEmpty(title))
        //        continue;

        //    var existingResource = existingResources.FirstOrDefault(r => r.EntityId == user.Id.ToString() && r.Title == title);
        //    if (existingResource != null)
        //    {
        //        existingResource.Description = description;
        //        existingResource.Data = new BsonDocument {
        //                { "Source", "Github" },
        //                { "Url", homepage ?? "" },
        //                { "Readme", readme ?? "" },
        //                { "Abstract", abst ?? "" },
        //                { "Keywords", keywords != null? string.Join("," ,keywords) :"" }
        //            };
        //        existingResource.UpdatedUTC = DateTime.UtcNow;
        //        await resourceRepository.UpdateAsync(existingResource);
        //    }
        //    else
        //    {
        //        var resource = new Resource
        //        {
        //            Title = title,
        //            Description = description,
        //            Data = new BsonDocument {
        //                { "Source", "Github" },
        //                { "Url", homepage ?? "" },
        //                { "Readme", readme ?? "" },
        //                { "Abstract", abst ?? "" },
        //                { "Keywords", keywords != null? string.Join("," ,keywords) :"" }
        //            },
        //            EntityId = user.Id.ToString(),
        //            CreatedUTC = DateTime.UtcNow,
        //        };

        //        var feed = new Feed()
        //        {
        //            CreatedUTC = resource.CreatedUTC,
        //            CreatedByUserId = resource.CreatedByUserId,
        //            Type = FeedType.Resource,
        //        };

        //        var id = await feedRepository.CreateAsync(feed);

        //        //mark feed write
        //        await userFeedRecordRepository.SetFeedWrittenAsync(resource.CreatedByUserId, id, DateTime.UtcNow);

        //        //create resource
        //        resource.Id = ObjectId.Parse(id);
        //        resource.UpdatedUTC = resource.CreatedUTC;
        //        await resourceRepository.CreateAsync(resource);
        //    }
        //}

        //// papers
        //var papers = json["paper_list"]?["papers"]?.AsArray();
        //Console.WriteLine($"{papers?.Count} papers");
        //foreach (JsonNode paperJson in papers ?? new JsonArray())
        //{
        //    var paperId = paperJson.AsObject().ContainsKey("id") ? paperJson["id"]?.GetValue<string>() : null;
        //    var paperSource = paperJson.AsObject().ContainsKey("source") ? paperJson["source"]?.GetValue<string>() : null;

        //    if (string.IsNullOrEmpty(paperId))
        //        continue;

        //    if (string.IsNullOrEmpty(paperSource))
        //        continue;

        //    if (existingPapers.Any(p => p.FeedEntityId == user.Id.ToString() && p.SourceId == paperId))
        //        continue;

        //    paperId = paperId.Replace("https://doi.org", string.Empty);

        //    Paper paper = null;
        //    switch (paperSource)
        //    {
        //        case "openalex":
        //            var oaPaper = await openAlexFacade.GetWorkAsync(paperId);
        //            if (oaPaper == null)
        //                continue;

        //            paper = new Paper
        //            {
        //                Authors = string.Join(", ", oaPaper.Authorships.Select(a => a.Author.DisplayName)),
        //                ExternalId = oaPaper.Doi,
        //                ExternalSystem = ExternalSystem.OpenAlex,
        //                Journal = oaPaper.PrimaryLocation?.Source?.DisplayName,
        //                OpenAccessPdfUrl = oaPaper.BestOaLocation?.PdfUrl,
        //                PublicationDate = oaPaper.PublicationDate ?? oaPaper.PublicationYear?.ToString(),
        //                Summary = FormatOAWorkAbstract(oaPaper.AbstractInvertedIndex),
        //                Title = oaPaper.Title,
        //            };
        //            break;

        //        case "semantic scholar":
        //            var ssPaper = await semanticScholarFacade.GetWorkAsync(paperId);
        //            if (ssPaper == null)
        //                continue;

        //            paper = new Paper
        //            {
        //                Authors = string.Join(", ", ssPaper.Authors?.Select(a => a.Name) ?? new List<string>()),
        //                ExternalId = ssPaper.ExternalIds.DOI,
        //                ExternalSystem = ExternalSystem.SemanticScholar,
        //                Journal = ssPaper.Journal?.Name,
        //                OpenAccessPdfUrl = ssPaper.OpenAccessPdf?.Url,
        //                PublicationDate = ssPaper.PublicationDate ?? ssPaper.Year?.ToString(),
        //                Summary = ssPaper.Abstract,
        //                Title = ssPaper.Title,
        //            };
        //            break;

        //        case "orcid":
        //            var oPaper = await orcidFacade.GetWorkFromDOI(paperId);
        //            if (oPaper == null)
        //                continue;

        //            paper = new Paper
        //            {
        //                Authors = string.Join(", ", oPaper.Contributors.Select(a => a.Name)),
        //                ExternalId = oPaper.ExternalIds?.FirstOrDefault()?.UrlValue,
        //                ExternalSystem = ExternalSystem.SemanticScholar,
        //                Journal = oPaper.JournalTitle,
        //                PublicationDate = oPaper.PublicationDate,
        //                Summary = oPaper.Description,
        //                Title = oPaper.Title,
        //            };
        //            break;
        //        default:
        //            continue;
        //    }

        //    paper.FeedId = user.Id.ToString();
        //    paper.CreatedUTC = DateTime.UtcNow;
        //    paper.DefaultVisibility = FeedEntityVisibility.Comment;
        //    paper.SourceId = paperId;
        //    paper.Type = PaperType.Article;
        //    paper.Status = ContentEntityStatus.Active;

        //    var feed = new Feed()
        //    {
        //        CreatedUTC = paper.CreatedUTC,
        //        CreatedByUserId = paper.CreatedByUserId,
        //        Type = FeedType.Paper,
        //    };

        //    var id = await feedRepository.CreateAsync(feed);

        //    //mark feed write
        //    await userFeedRecordRepository.SetFeedWrittenAsync(paper.CreatedByUserId, id, DateTime.UtcNow);

        //    //create paper
        //    paper.Id = ObjectId.Parse(id);
        //    paper.UpdatedUTC = paper.CreatedUTC;
        //    await paperRepository.CreateAsync(paper);
        //}

        //var existingMembership = existingMemberships.FirstOrDefault(m => m.UserId == user.Id.ToString() && m.CommunityEntityId == "674f3ceda442a3424df80b05");
        //if (existingMembership == null)
        //{
        //    //associate to hub
        //    await membershipRepository.CreateAsync(new Membership
        //    {
        //        AccessLevel = AccessLevel.Member,
        //        CommunityEntityId = "674f3ceda442a3424df80b05",
        //        CommunityEntityType = CommunityEntityType.Node,
        //        UserId = user.Id.ToString(),
        //        CreatedByUserId = user.Id.ToString(),
        //        CreatedUTC = DateTime.UtcNow,
        //    });
        //}

        Console.WriteLine($"Done with {user.FirstName} {user.LastName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error" + ex.ToString());
        continue;
    }
}

string FormatOAWorkAbstract(Dictionary<string, List<int>> invertedIndex)
{
    if (invertedIndex == null)
        return null;

    StringBuilder stringBuilder = new();
    int maxPosition = invertedIndex.Values.SelectMany(positions => positions).Max();

    for (int position = 0; position <= maxPosition; position++)
    {
        var wordAtPosition = invertedIndex
            .Where(wordInfo => wordInfo.Value.Contains(position))
            .Select(wordInfo => wordInfo.Key)
            .SingleOrDefault();

        stringBuilder.Append($"{wordAtPosition} ");
    }

    return stringBuilder.ToString().Trim();
}

string FormatPMArticleAbstract(List<AbstractText> abstractFragments)
{
    if (abstractFragments == null)
        return null;

    if (abstractFragments?.Count > 1)
        return string.Join("\n", abstractFragments.Select(at => $"{at?.Label}: {at?.Text}"));

    return abstractFragments?.First()?.Text;
}


await host.StopAsync();
host.Dispose();
Console.WriteLine("Done");
Console.ReadLine();