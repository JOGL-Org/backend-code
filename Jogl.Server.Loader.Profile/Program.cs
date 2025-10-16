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
using Jogl.Server.Loader.Profile.DTO;
using MongoDB.Bson;

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
//var paperService = host.Services.GetRequiredService<IPaperService>();
var resourceService = host.Services.GetRequiredService<IResourceService>();
var membershipService = host.Services.GetRequiredService<IMembershipService>();

var existingUsers = userService.List();
//var existingPapers = paperService.List();
var existingResources = resourceService.List();
//var solanaNode = "650dc1f8092fdb774ae05ff0";

foreach (var file in Directory.GetFiles("../../../solana/"))
{
    try
    {
        var jsonString = File.ReadAllText(file);
        var importUser = JsonSerializer.Deserialize<UserSolana>(jsonString);
        //if (string.IsNullOrEmpty(importUser.Email))
        //{
        //    Console.WriteLine($"{importUser.FirstName} {importUser.LastName} has no email");
        //    continue;
        //}

        if (importUser.ColloseumProfile == null)
            continue;

        var user = existingUsers.SingleOrDefault(u => importUser.ColloseumProfile.Username == u.Username);
        if (user == null)
        {
            user = new User
            {
                //Bio = importUser.Bio,
                //ShortBio = importUser.Headline,
                CreatedUTC = DateTime.UtcNow,
                //Current = importUser.LatestActivities,
                //FirstName = importUser.FirstName,
                //LastName = importUser.LastName,
                Email = importUser.ColloseumProfile.Username + "@colloseum.org",
                Username = importUser.ColloseumProfile.Username,
                //Experience = importUser.Experience?.Select(e => new UserExperience
                //{
                //    Company = e.Company,
                //    Description = e.Description,
                //    Position = e.Position,
                //    DateFrom = e.DateFrom?.ToString(),
                //    DateTo = e.DateTo?.ToString(),
                //    Current = e.Current,
                //})?.ToList(),
                //Education = importUser.Education?.Select(e => new UserEducation
                //{
                //    Program = e.Program,
                //    Description = e.Description,
                //    School = e.School,
                //    DateFrom = e.DateFrom?.ToString(),
                //    DateTo = e.DateTo?.ToString(),
                //    Current = e.Current,
                //})?.ToList(),
                //Skills = importUser.Skills?.Select(s => s.SkillName)?.ToList(),
                Status = UserStatus.Verified
            };

            var userId = await userService.CreateAsync(user);
            user.Id = ObjectId.Parse(userId);

            //await membershipService.CreateAsync(new Membership
            //{
            //    CommunityEntityId = solanaNode,
            //    CommunityEntityType = CommunityEntityType.Node,
            //    CreatedByUserId = userId,
            //    CreatedUTC = DateTime.UtcNow,
            //    UserId = userId
            //});
        }

        ////papers
        //foreach (var importPaper in importUser.Papers)
        //{
        //    if (importPaper.Doi == null)
        //        continue;

        //    if (existingPapers.Any(p => p.ExternalId == importPaper.Doi))
        //        continue;

        //    await paperService.CreateAsync(new Paper
        //    {
        //        Authors = importPaper.AuthorList,
        //        CreatedUTC = DateTime.UtcNow,
        //        DefaultVisibility = FeedEntityVisibility.View,
        //        ExternalId = importPaper.Doi,
        //        ExternalSystem = !string.IsNullOrEmpty(importPaper.SemanticScholarId) ? ExternalSystem.SemanticScholar : !string.IsNullOrEmpty(importPaper.OpenAlexId) ? ExternalSystem.OpenAlex : ExternalSystem.None,
        //        Journal = importPaper.Location,
        //        Title = importPaper.Title,
        //        Summary = importPaper.Abstract,
        //        SourceId = importPaper.SemanticScholarId ?? importPaper.OpenAlexId,
        //        PublicationDate = importPaper.PublicationDate,
        //        FeedId = user.Id.ToString()
        //    });
        //}

        //repos
        var existingRepos = existingResources.Where(er => er.Type == ResourceType.Repository).ToList();
        if (importUser.Repos != null)
            foreach (var importRepo in importUser.Repos)
            {
                if (existingRepos.Any(p => p.Title == importRepo.Name))
                    continue;

                await resourceService.CreateAsync(new Resource
                {
                    Description = importRepo.Abstract,
                    EntityId = user.Id.ToString(),
                    Type = ResourceType.Patent,
                    CreatedUTC = DateTime.UtcNow,
                    DefaultVisibility = FeedEntityVisibility.View,
                    Title = importRepo.Name,
                    Data = new BsonDocument {
                        { "License", importRepo.License ?? "" },
                        { "Source", "Github" },
                        { "Url", importRepo.Url ?? "" },
                        { "Readme", importRepo.Readme ?? "" },
                        { "Language", importRepo.Language ?? "" },
                        { "Keywords", string.Join(",", importRepo.Topics ?? new List<string>()) }
                    },
                });
            }


        ////patents
        //var existingPatents = existingResources.Where(er => er.Type == ResourceType.Patent).ToList();
        //if (importUser.Patents?.PatentFamilies != null)
        //    foreach (var importPatent in importUser.Patents.PatentFamilies)
        //    {
        //        if (existingPatents.Any(p => p["PatentId"] == importPatent.Id))
        //            continue;

        //        await resourceService.CreateAsync(new Resource
        //        {
        //            Description = importPatent.Abstract,
        //            Type = ResourceType.Patent,
        //            CreatedUTC = DateTime.UtcNow,
        //            DefaultVisibility = FeedEntityVisibility.View,
        //            Title = importPatent.Title,
        //            Data = new MongoDB.Bson.BsonDocument[]
        //        });
        //    }


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