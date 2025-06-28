using Jogl.Server.AI;
using Jogl.Server.Configuration;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Lix;
using Jogl.Server.SerpAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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

//var calendarService = new GoogleCalendarService(new UrlService(config), null, config, new Logger<CalendarService>(new LoggerFactory()));
//delete some events
//await DeleteEventAsync("682b50076363951c53aac2b1");

//async Task DeleteEventAsync(string eventId)
//{
//    var ev = eventRepository.Get(eventId);
//    var externalCalendarId = await calendarService.GetJoglCalendarAsync();
//    await calendarService.DeleteEventAsync(externalCalendarId, ev.ExternalId);

//    var attendances = eventAttendanceRepository.List(a => a.EventId == eventId && !a.Deleted);
//    foreach (var a in attendances)
//    {
//        await eventAttendanceRepository.DeleteAsync(a);
//    }

//    await feedRepository.DeleteAsync(eventId);
//    await eventRepository.DeleteAsync(eventId);
//}
//return;
////load synthetic data
//var json = File.ReadAllText("C:\\code\\Seed_data_synbio.json");
//var data = JsonSerializer.Deserialize<List<User>>(json);
//foreach (var user in data)
//{
//    try
//    {
//        user.Status = UserStatus.Verified;

//        var id = await userRepository.CreateAsync(user);
//        foreach (var paper in user.Papers)
//        {
//            paper.FeedId = id;
//            paper.DefaultVisibility = FeedEntityVisibility.View;
//            await paperRepository.CreateAsync(paper);
//        }

//        foreach (var doc in user.Documents)
//        {
//            doc.FeedId = id;
//            doc.Type = DocumentType.JoglDoc;
//            doc.DefaultVisibility = FeedEntityVisibility.View;
//            await documentRepository.CreateAsync(doc);
//        }

//        foreach (var res in user.Resources)
//        {
//            res.EntityId = id;
//            res.DefaultVisibility = FeedEntityVisibility.View;
//            await resourceRepository.CreateAsync(res);
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex);
//    }
//}

//load aphp data
var perplexityService = new PerplexityAIService(config);
var serpAPIFacade = new SerpAPIFacade(config, new MemoryCache(new MemoryCacheOptions()), LoggerFactory.Create(c => { }).CreateLogger<SerpAPIFacade>());
var lixFacade = new LixFacade(config, new MemoryCache(new MemoryCacheOptions()), LoggerFactory.Create(c => { }).CreateLogger<LixFacade>());
var downloader = new DocumentDownloader();
var imageService = new ImageService(new ImageRepository(config), new BlobStorageService(config));

var users = new List<User>();
var openAlexFacade = new OpenAlexFacade(config);
var semanticScholarFacade = new SemanticScholarFacade(config);
var orcidFacade = new OrcidFacade(config, null);


var existingMemberships = membershipRepository.List(m => !m.Deleted);
var existingResources = resourceRepository.List(r => !r.Deleted);
var existingPapers = paperRepository.List(p => !p.Deleted);
var existingChannels = channelRepository.Query(c => !string.IsNullOrEmpty(c.Key)).ToList();

foreach (var file in Directory.GetFiles("../../../WIC2025/"))
{
    try
    {
        var jsonString = File.ReadAllText(file);
        var json = JsonNode.Parse(jsonString);
        var firstName = json["first_name"]?.GetValue<string>();
        var lastName = json["last_name"]?.GetValue<string>();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            Console.WriteLine($"Skipping file {file}; name missing");
            continue;
        }

        var user = new User
        {
            FirstName = json["first_name"].GetValue<string>(),
            LastName = json["last_name"].GetValue<string>(),
            ShortBio = json["Headline"]?.GetValue<string>(),
            Bio = json["Bio"]?.GetValue<string>(),
            Experience = json["Experience"]?.Deserialize<List<UserExperience>>(),
            Education = json["Education"]?.Deserialize<List<UserEducation>>(),
            Username = json["first_name"].GetValue<string>() + json["last_name"].GetValue<string>(),
            Email = $"{json["email"]}"
        };

        //skills
        var skillsArray = json["Skills"]?.AsArray();
        user.Skills = new List<string>();
        foreach (JsonNode skillNode in skillsArray ?? new JsonArray())
        {
            var skill = skillNode["skill"].GetValue<string>();
            user.Skills.Add(skill);
        }

        //create if needed
        var existingUser = userRepository.Get(u => u.FirstName == user.FirstName && u.LastName == user.LastName);
        if (existingUser == null)
        {
            var feed = new Feed()
            {
                CreatedUTC = user.CreatedUTC,
                CreatedByUserId = user.CreatedByUserId,
                Type = FeedType.User
            };

            var id = await feedRepository.CreateAsync(feed);
            user.Id = ObjectId.Parse(id);
            user.Onboarding = true;

            user.Status = UserStatus.Verified;
            user.ContactMe = true;
            user.NotificationSettings = new UserNotificationSettings
            {
                ContainerInvitationEmail = true,
                ContainerInvitationJogl = true,
                DocumentMemberContainerEmail = true,
                DocumentMemberContainerJogl = true,
                EventInvitationJogl = true,
                EventMemberContainerEmail = true,
                EventMemberContainerJogl = true,
                MentionEmail = true,
                MentionJogl = true,
                NeedMemberContainerEmail = true,
                NeedMemberContainerJogl = true,
                PaperMemberContainerEmail = true,
                PaperMemberContainerJogl = true,
                PostAttendingEventEmail = true,
                PostAttendingEventJogl = true,
                PostAuthoredEventEmail = true,
                PostAuthoredEventJogl = true,
                PostAuthoredObjectEmail = true,
                PostAuthoredObjectJogl = true,
                PostMemberContainerEmail = true,
                PostMemberContainerJogl = true,
                ThreadActivityEmail = true,
                ThreadActivityJogl = true,
            };

            await userRepository.CreateAsync(user);

            //create AI channel
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
                CommunityEntityType = CommunityEntityType.Channel,
                UserId = user.Id.ToString(),
                CreatedByUserId = user.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
            });
        }
        else
        {
            user = existingUser;
            user.Onboarding = true;

            await userRepository.SetOnboardingStatusAsync(user);
        }

        // repositories
        var repos = json["repolist"]?.AsArray();
        Console.WriteLine($"{repos?.Count} repositories");
        foreach (JsonNode repo in repos ?? new JsonArray())
        {
            var title = repo.AsObject().ContainsKey("name") ? repo["name"]?.GetValue<string>() : null;
            var description = repo.AsObject().ContainsKey("description") ? repo["description"]?.GetValue<string>() : null;
            var abst = repo.AsObject().ContainsKey("abstract") ? repo["abstract"]?.GetValue<string>() : null;
            var keywords = repo.AsObject().ContainsKey("keywords") ? (repo["keywords"] as JsonArray).Select(node => node.GetValue<string>()).ToList() : null;
            var readme = repo.AsObject().ContainsKey("readme") ? repo["readme"]?.GetValue<string>() : null;
            var homepage = repo.AsObject().ContainsKey("homepage") ? repo["homepage"]?.GetValue<string>() : null;

            if (string.IsNullOrEmpty(title))
                continue;

            var existingResource = existingResources.FirstOrDefault(r => r.EntityId == user.Id.ToString() && r.Title == title);
            if (existingResource != null)
            {
                existingResource.Description = description;
                existingResource.Data = new BsonDocument {
                        { "Source", "Github" },
                        { "Url", homepage ?? "" },
                        { "Readme", readme ?? "" },
                        { "Abstract", abst ?? "" },
                        { "Keywords", keywords != null? string.Join("," ,keywords) :"" }
                    };
                existingResource.UpdatedUTC = DateTime.UtcNow;
                await resourceRepository.UpdateAsync(existingResource);
            }
            else
            {
                var resource = new Resource
                {
                    Title = title,
                    Description = description,
                    Data = new BsonDocument {
                        { "Source", "Github" },
                        { "Url", homepage ?? "" },
                        { "Readme", readme ?? "" },
                        { "Abstract", abst ?? "" },
                        { "Keywords", keywords != null? string.Join("," ,keywords) :"" }
                    },
                    EntityId = user.Id.ToString(),
                    CreatedUTC = DateTime.UtcNow,
                };

                var feed = new Feed()
                {
                    CreatedUTC = resource.CreatedUTC,
                    CreatedByUserId = resource.CreatedByUserId,
                    Type = FeedType.Resource,
                };

                var id = await feedRepository.CreateAsync(feed);

                //mark feed write
                await userFeedRecordRepository.SetFeedWrittenAsync(resource.CreatedByUserId, id, DateTime.UtcNow);

                //create resource
                resource.Id = ObjectId.Parse(id);
                resource.UpdatedUTC = resource.CreatedUTC;
                await resourceRepository.CreateAsync(resource);
            }
        }

        // papers
        var papers = json["paper_list"]?["papers"]?.AsArray();
        Console.WriteLine($"{papers?.Count} papers");
        foreach (JsonNode paperJson in papers ?? new JsonArray())
        {
            var paperId = paperJson.AsObject().ContainsKey("id") ? paperJson["id"]?.GetValue<string>() : null;
            var paperSource = paperJson.AsObject().ContainsKey("source") ? paperJson["source"]?.GetValue<string>() : null;

            if (string.IsNullOrEmpty(paperId))
                continue;

            if (string.IsNullOrEmpty(paperSource))
                continue;

            if (existingPapers.Any(p => p.FeedEntityId == user.Id.ToString() && p.SourceId == paperId))
                continue;

            paperId = paperId.Replace("https://doi.org", string.Empty);

            Paper paper = null;
            switch (paperSource)
            {
                case "openalex":
                    var oaPaper = await openAlexFacade.GetWorkAsync(paperId);
                    if (oaPaper == null)
                        continue;

                    paper = new Paper
                    {
                        Authors = string.Join(", ", oaPaper.Authorships.Select(a => a.Author.DisplayName)),
                        ExternalId = oaPaper.Doi,
                        ExternalSystem = ExternalSystem.OpenAlex,
                        Journal = oaPaper.PrimaryLocation?.Source?.DisplayName,
                        OpenAccessPdfUrl = oaPaper.BestOaLocation?.PdfUrl,
                        PublicationDate = oaPaper.PublicationDate ?? oaPaper.PublicationYear?.ToString(),
                        Summary = FormatOAWorkAbstract(oaPaper.AbstractInvertedIndex),
                        Title = oaPaper.Title,
                    };
                    break;

                case "semantic scholar":
                    var ssPaper = await semanticScholarFacade.GetWorkAsync(paperId);
                    if (ssPaper == null)
                        continue;

                    paper = new Paper
                    {
                        Authors = string.Join(", ", ssPaper.Authors?.Select(a => a.Name) ?? new List<string>()),
                        ExternalId = ssPaper.ExternalIds.DOI,
                        ExternalSystem = ExternalSystem.SemanticScholar,
                        Journal = ssPaper.Journal?.Name,
                        OpenAccessPdfUrl = ssPaper.OpenAccessPdf?.Url,
                        PublicationDate = ssPaper.PublicationDate ?? ssPaper.Year?.ToString(),
                        Summary = ssPaper.Abstract,
                        Title = ssPaper.Title,
                    };
                    break;

                case "orcid":
                    var oPaper = await orcidFacade.GetWorkFromDOI(paperId);
                    if (oPaper == null)
                        continue;

                    paper = new Paper
                    {
                        Authors = string.Join(", ", oPaper.Contributors.Select(a => a.Name)),
                        ExternalId = oPaper.ExternalIds?.FirstOrDefault()?.UrlValue,
                        ExternalSystem = ExternalSystem.SemanticScholar,
                        Journal = oPaper.JournalTitle,
                        PublicationDate = oPaper.PublicationDate,
                        Summary = oPaper.Description,
                        Title = oPaper.Title,
                    };
                    break;
                default:
                    continue;
            }

            paper.FeedId = user.Id.ToString();
            paper.CreatedUTC = DateTime.UtcNow;
            paper.DefaultVisibility = FeedEntityVisibility.Comment;
            paper.SourceId = paperId;
            paper.Type = PaperType.Article;
            paper.Status = ContentEntityStatus.Active;

            var feed = new Feed()
            {
                CreatedUTC = paper.CreatedUTC,
                CreatedByUserId = paper.CreatedByUserId,
                Type = FeedType.Paper,
            };

            var id = await feedRepository.CreateAsync(feed);

            //mark feed write
            await userFeedRecordRepository.SetFeedWrittenAsync(paper.CreatedByUserId, id, DateTime.UtcNow);

            //create paper
            paper.Id = ObjectId.Parse(id);
            paper.UpdatedUTC = paper.CreatedUTC;
            await paperRepository.CreateAsync(paper);
        }

        var existingMembership = existingMemberships.FirstOrDefault(m => m.UserId == user.Id.ToString() && m.CommunityEntityId == "674f3ceda442a3424df80b05");
        if (existingMembership == null)
        {
            //associate to hub
            await membershipRepository.CreateAsync(new Membership
            {
                AccessLevel = AccessLevel.Member,
                CommunityEntityId = "674f3ceda442a3424df80b05",
                CommunityEntityType = CommunityEntityType.Node,
                UserId = user.Id.ToString(),
                CreatedByUserId = user.Id.ToString(),
                CreatedUTC = DateTime.UtcNow,
            });
        }

        Console.WriteLine($"done with {user.FirstName} {user.LastName}");
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


Console.WriteLine("Done");
Console.ReadLine();