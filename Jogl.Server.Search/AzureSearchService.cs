using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Jogl.Server.Data;
using User = Jogl.Server.Search.Model.User;
using System.Globalization;
using MongoDB.Bson;

namespace Jogl.Server.Search
{
    public class AzureSearchService : ISearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureSearchService> _logger;

        public AzureSearchService(IConfiguration configuration, ILogger<AzureSearchService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task IndexUsersAsync(IEnumerable<Data.User> users, IEnumerable<Document> documents, IEnumerable<Paper> papers, IEnumerable<Resource> resources)
        {
            SearchClient searchClient = new SearchClient(
                new Uri(_configuration["Azure:Search:URL"]),
                "users",
                new DefaultAzureCredential());


            foreach (var batch in users.Select(u => TransformUserToSearchDocument(u, documents.Where(d => d.FeedId == u.Id.ToString()), papers.Where(p => p.FeedId == u.Id.ToString()), resources.Where(r => r.EntityId == u.Id.ToString()))).Chunk(100))
            {
                await searchClient.UploadDocumentsAsync(batch);
            }
        }

        private SearchDocument TransformUserToSearchDocument(Data.User user, IEnumerable<Document> documents, IEnumerable<Paper> papers, IEnumerable<Resource> resources)
        {
            var searchDoc = new SearchDocument();

            var repositories = resources?.Where(r => r.Type == ResourceType.Repository)?.ToList();

            // Map simple fields
            searchDoc["id"] = user.Id.ToString();
            searchDoc["Email"] = user.Email ?? "";
            searchDoc["Name"] = user.FirstName + " " + user.LastName;
            searchDoc["ShortBio"] = user.ShortBio ?? string.Empty;
            searchDoc["Bio"] = user.Bio ?? string.Empty;
            searchDoc["Current"] = user.Current ?? string.Empty;
            //searchDoc["Educations_Institution"] = user.Education != null ? user.Education.Select(e => e.School).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            //searchDoc["Educations_Program"] = user.Education != null ? user.Education.Select(e => e.Program).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            searchDoc["Location"] = user.Country ?? string.Empty;
            searchDoc["Current_Roles"] = user.Experience != null ? user.Experience.Where(e => e.Current).Select(e => e.Position).Where(str => !string.IsNullOrEmpty(str)) : new List<string>();
            searchDoc["Past_Roles"] = user.Experience != null ? user.Experience.Where(e => !e.Current).Select(e => e.Position).Where(str => !string.IsNullOrEmpty(str)) : new List<string>(); ;
            searchDoc["Current_Companies"] = user.Experience != null ? user.Experience.Where(e => e.Current).Select(e => e.Company).Distinct().Where(str => !string.IsNullOrEmpty(str)) : new List<string>();
            searchDoc["Past_Companies"] = user.Experience != null ? user.Experience.Where(e => !e.Current).Select(e => e.Company).Distinct().Where(str => !string.IsNullOrEmpty(str)) : new List<string>();
            searchDoc["Study_Programs"] = user.Education != null ? user.Education.Select(e => e.Program).Distinct().Where(str => !string.IsNullOrEmpty(str)) : new List<string>();
            searchDoc["Study_Institutions"] = user.Education != null ? user.Education.Select(e => e.School).Distinct().Where(str => !string.IsNullOrEmpty(str)) : new List<string>();

            searchDoc["Labels"] = GenerateLabels(user, documents, papers, repositories);

            searchDoc["Documents_Title"] = documents != null ? documents.Select(e => e.Name).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Documents_Content"] = documents != null ? documents.Select(e => e.Description).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            searchDoc["Repositories_Title"] = repositories != null ? repositories.Select(e => e.Title).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Repositories_Languages"] = repositories != null ? repositories.Select(e => e["Language"]).Where(str => !string.IsNullOrEmpty(str)).Distinct().ToList() : new List<string>();
            searchDoc["Repositories_Keywords"] = repositories != null ? repositories.Select(e => e["Keywords"]).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            //searchDoc["Projects_Title"] = projects != null ? projects.Select(e => e.Title).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            //searchDoc["Projects_Languages"] = projects != null ? projects.Select(e => e["Language"]).Where(str => !string.IsNullOrEmpty(str)).Distinct().ToList() : new List<string>();

            searchDoc["Papers_Title"] = papers != null ? papers.Select(e => e.Title).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Papers_Abstract"] = papers != null ? papers.Select(e => e.Summary).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            searchDoc["Papers_Title_Current"] = papers != null ? papers.Where(IsCurrent).Select(e => e.Title).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Papers_Abstract_Current"] = papers != null ? papers.Where(IsCurrent).Select(e => e.Summary).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            return searchDoc;
        }

        private List<string> GenerateLabels(Data.User user, IEnumerable<Document> documents, IEnumerable<Paper> papers, IEnumerable<Resource> repositories)
        {
            var res = new List<string>();
            if (user.Experience?.Any(e => e.Current == true) == true)
                res.Add("Student");

            if (repositories?.Any() == true)
                res.Add("Developer");

            if (papers?.Any() == true)
                res.Add("Researcher");

            return res;
        }

        private bool IsCurrent(Paper p)
        {
            if (!DateTime.TryParseExact(p.PublicationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return false;

            return (DateTime.UtcNow - date) < TimeSpan.FromDays(2 * 365);
        }

        public async Task<List<SearchResult<User>>> SearchUsersAsync(string query, string configuration = "default", IEnumerable<string>? userIds = default)
        {
            // Create a search client
            SearchClient searchClient = new SearchClient(
                new Uri(_configuration["Azure:Search:URL"]),
                "users",
                new DefaultAzureCredential());

            //create options
            var options = new SearchOptions
            {
                QueryType = SearchQueryType.Semantic,
                SemanticSearch = new SemanticSearchOptions
                {
                    SemanticConfigurationName = configuration,
                    QueryCaption = new QueryCaption(QueryCaptionType.Extractive),
                    QueryAnswer = new QueryAnswer(QueryAnswerType.Extractive)
                },
            };

            //if ids available, generate filter
            if (userIds != null && userIds.Any())
                options.Filter = $"search.in(id, '{string.Join(",", userIds.Select(id => $"{id}"))}')";

            try
            {
                //execute search
                var response = await searchClient.SearchAsync<User>(query, options);

                var filteredResults = new List<SearchResult<User>>();
                await foreach (SearchResult<User> result in response.Value.GetResultsAsync())
                {
                    if (result.SemanticSearch.RerankerScore.HasValue && result.SemanticSearch.RerankerScore.Value >= 1.75d)
                    {
                        filteredResults.Add(result);
                    }
                }

                return filteredResults;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
