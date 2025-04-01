using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Jogl.Server.Data;

namespace Jogl.Server.Search
{
    public class AzureSearchService : ISearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<User> _logger;

        public AzureSearchService(IConfiguration configuration, ILogger<User> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task IndexUsersAsync(IEnumerable<Data.User> users, IEnumerable<Document> documents, IEnumerable<Paper> papers)
        {
            SearchClient searchClient = new SearchClient(
                new Uri(_configuration["Azure:Search:URL"]),
                "users",
                new DefaultAzureCredential());

            var searchDocuments = users.Select(u => TransformUserToSearchDocument(u, documents.Where(d => d.FeedId == u.Id.ToString()), papers.Where(p => p.FeedId == u.Id.ToString()))).ToList();
            await searchClient.UploadDocumentsAsync(searchDocuments);
        }

        private SearchDocument TransformUserToSearchDocument(Data.User user, IEnumerable<Document> documents, IEnumerable<Paper> papers)
        {
            var searchDoc = new SearchDocument();

            // Map simple fields
            searchDoc["id"] = user.Id.ToString();
            searchDoc["Name"] = user.FirstName + " " + user.LastName;
            searchDoc["ShortBio"] = user.ShortBio ?? string.Empty;
            searchDoc["Bio"] = user.Bio ?? string.Empty;
            searchDoc["Educations_Institution"] = user.Education != null ? user.Education.Select(e => e.School).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Educations_Program"] = user.Education != null ? user.Education.Select(e => e.Program).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Experiences_Company"] = user.Experience != null ? user.Experience.Select(e => e.Company).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Experiences_Position"] = user.Experience != null ? user.Experience.Select(e => e.Position).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            searchDoc["Documents_Title"] = documents != null ? documents.Select(e => e.Name).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Documents_Content"] = documents != null ? documents.Select(e => e.Description).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            searchDoc["Papers_Title"] = papers != null ? papers.Select(e => e.Title).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();
            searchDoc["Papers_Abstract"] = papers != null ? papers.Select(e => e.Summary).Where(str => !string.IsNullOrEmpty(str)).ToList() : new List<string>();

            return searchDoc;
        }

        public async Task<List<User>> SearchUsersAsync(string query, IEnumerable<string>? userIds = default)
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
                Size = 3,
                SemanticSearch = new SemanticSearchOptions
                {
                    SemanticConfigurationName = "default",
                }
            };

            //if ids available, generate filter
            if (userIds != null && userIds.Any())
                options.Filter = string.Join(" or ", userIds.Select(id => $"id eq '{id}'"));
            
            try
            {

                //execute search
                var results = await searchClient.SearchAsync<User>(query, options);

                return results.Value.GetResultsAsync().ToBlockingEnumerable().Select(v => v.Document).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
