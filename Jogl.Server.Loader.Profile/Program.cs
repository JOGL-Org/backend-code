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
using System.Drawing.Text;

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
var solanaNode = "684bf7e14b1507a00823d69f";

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
                Bio = importUser.Bio,
                ShortBio = importUser.Headline,
                StatusText = importUser.LookingForCollab ? "Looking for collaboration" : string.Empty,
                CreatedUTC = DateTime.UtcNow,
                Current = importUser.RecentGithubActivity,
                FirstName = importUser.FirstName,
                City = importUser.Location,
                LastName = importUser.LastName,
                Email = importUser.ColloseumProfile.Username + "@colloseum.org",
                Username = importUser.ColloseumProfile.Username,
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
                Skills = importUser.Skills ?? new List<string>(),
                Status = UserStatus.Verified,
                Contacts = new Dictionary<string, string>()
            };

            AmendContacts(user.Contacts, importUser);
            
            var userId = await userService.CreateAsync(user);
            user.Id = ObjectId.Parse(userId);

            await membershipService.CreateAsync(new Membership
            {
                CommunityEntityId = solanaNode,
                CommunityEntityType = CommunityEntityType.Node,
                CreatedByUserId = userId,
                CreatedUTC = DateTime.UtcNow,
                UserId = userId
            });
        }

        //else
        //{
        //    if (user.Contacts == null)
        //        user.Contacts = new Dictionary<string, string>();

        //    AmendContacts(user.Contacts, importUser);
        //    await userService.UpdateAsync(user);
        //}

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
                if (importRepo == null)
                    continue;

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
                        { "Keywords", string.Join(",", importRepo.Topics?.Select(t=>t.Topic.Name) ?? new List<string>()) }
                    },
                });
            }

        //projects
        var existingProjects = existingResources.Where(er => er.Type == ResourceType.Project).ToList();
        if (importUser.Projects != null)
            foreach (var importProject in importUser.Projects)
            {
                if (importProject == null)
                    continue;

                if (existingProjects.Any(p => p.Data["Id"] == importProject.Id && p.EntityId == user.Id.ToString()))
                    continue;

                await resourceService.CreateAsync(new Resource
                {
                    Description = importProject.Description,
                    EntityId = user.Id.ToString(),
                    Type = ResourceType.Project,
                    CreatedUTC = DateTime.UtcNow,
                    DefaultVisibility = FeedEntityVisibility.View,
                    Title = importProject.Title,
                    Data = new BsonDocument {
                        { "Id", importProject.Id.ToString() },
                        { "Source", importProject.Source??string.Empty },
                        { "Url",importProject.RepoLink??string.Empty },
                        { "Date", importProject.Date??string.Empty },
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
        //Console.WriteLine($"Error" + ex.ToString());
        continue;
    }
}

void AmendContacts(Dictionary<string, string> contacts, UserSolana importUser)
{
    var res = new Dictionary<string, string>();

    if (!string.IsNullOrEmpty(importUser.SocialLinks?.ColloseumHandle) && contacts.ContainsKey("Colloseum"))
        contacts.Add("Colloseum", importUser.SocialLinks.ColloseumHandle);
    if (!string.IsNullOrEmpty(importUser.SocialLinks?.GithubHandle) && contacts.ContainsKey("Github"))
        contacts.Add("Github", importUser.SocialLinks.GithubHandle);
    if (!string.IsNullOrEmpty(importUser.SocialLinks?.LinkedinHandle) && contacts.ContainsKey("LinkedIn"))
        contacts.Add("LinkedIn", importUser.SocialLinks.LinkedinHandle);
    if (!string.IsNullOrEmpty(importUser.SocialLinks?.TelegramHandle) && contacts.ContainsKey("Telegram"))
        contacts.Add("Telegram", importUser.SocialLinks.TelegramHandle);
    if (!string.IsNullOrEmpty(importUser.SocialLinks?.TwitterHandle) && contacts.ContainsKey("Twitter"))
        contacts.Add("Twitter", importUser.SocialLinks.TwitterHandle);
}


await host.StopAsync();
host.Dispose();
Console.WriteLine("Done");
Console.ReadLine();