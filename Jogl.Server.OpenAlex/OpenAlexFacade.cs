using Jogl.Server.Data.Util;
using Jogl.Server.OpenAlex.DTO;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Jogl.Server.OpenAlex
{
    public class OpenAlexFacade : IOpenAlexFacade
    {
        private readonly IConfiguration _configuration;
        const int PAGE_SIZE = 200;

        public OpenAlexFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ListPage<Author>> ListAuthorsAsync(string search, int page, int pageSize)
        {
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest("authors");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");
            request.AddQueryParameter("per-page", pageSize);
            request.AddQueryParameter("page", page);
            request.AddQueryParameter("search", GetQueryFromSearch(search));

            var response = await client.ExecuteGetAsync<Response<Author>>(request);
            if (response.Data?.Results == null)
                return new ListPage<Author>(new List<Author>(), 0);

            var works = await ListWorksForAuthorIdsAsync(response.Data.Results.Select(r => r.Id), 1, PAGE_SIZE);
            foreach (var author in response.Data.Results)
            {
                author.LastWork = works.Items
                    .OrderByDescending(w => w.PublicationDate)
                    .FirstOrDefault(w => w.Authorships.Any(a => a.Author.Id == author.Id));
            }

            return new ListPage<Author>(response.Data.Results, response.Data.Meta.Count);
        }

        public async Task<ListPage<Work>> ListWorksForAuthorIdAsync(string authorId, int page, int pageSize)
        {
            return await ListWorksForAuthorIdsAsync(new[] { authorId }, page, pageSize);
        }

        public async Task<ListPage<Work>> ListWorksForAuthorIdsAsync(IEnumerable<string> authorIds, int page, int pageSize)
        {
            if (!authorIds.Any())
                return new ListPage<Work>(new List<Work>());

            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest("works");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");
            request.AddQueryParameter("per-page", pageSize);
            request.AddQueryParameter("page", page);
            request.AddQueryParameter("filter", $"authorships.author.id:authors/{string.Join('|', authorIds)}");
            request.AddQueryParameter("sort", "publication_date:desc");

            var response = await client.ExecuteGetAsync<Response<Work>>(request);
            foreach (var item in response?.Data?.Results)
            {
                item.Id = item.Id.Replace("https://openalex.org/", string.Empty);
            }

            return new ListPage<Work>(response?.Data?.Results ?? new List<Work>(), response.Data.Meta.Count);
        }

        public async Task<ListPage<Work>> ListWorksForAuthorNameAsync(string authorSearch, int page, int pageSize)
        {
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest("works");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");
            request.AddQueryParameter("per-page", pageSize);
            request.AddQueryParameter("page", page);
            request.AddQueryParameter("filter", $"display_name.search:{authorSearch}");

            var response = await client.ExecuteGetAsync<Response<Work>>(request);
            foreach (var item in response?.Data?.Results)
            {
                item.Id = item.Id.Replace("https://openalex.org/", string.Empty);
            }

            return new ListPage<Work>(response?.Data?.Results ?? new List<Work>(), response.Data.Meta.Count);
        }

        public async Task<ListPage<Work>> ListWorksAsync(string search, int page, int pageSize)
        {
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest("works");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");
            request.AddQueryParameter("per-page", pageSize);
            request.AddQueryParameter("page", page);
            request.AddQueryParameter("search", GetQueryFromSearch(search));

            var response = await client.ExecuteGetAsync<Response<Work>>(request);
            foreach (var item in response?.Data?.Results)
            {
                item.Id = item.Id.Replace("https://openalex.org/", string.Empty);
            }

            return new ListPage<Work>(response?.Data?.Results ?? new List<Work>(), response.Data.Meta.Count);
        }

        public async Task<Work> GetWorkAsync(string id)
        {
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest($"works/{id}");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");

            var response = await client.ExecuteGetAsync<Work>(request);
            response.Data.Id = response.Data.Id.Replace("https://openalex.org/", string.Empty);
            return response.Data;
        }

        private string GetQueryFromSearch(string search)
        {
            return search?.Replace(" ", " ")?.ToLower();
        }

        public async Task<Work> GetWorkFromDOI(string doi)
        {
            var formattedDOI = doi.Contains("doi.org") ? doi : $"https://doi.org/{doi}";
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest($"works/{formattedDOI}");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");

            var response = await client.ExecuteGetAsync<Work>(request);
            return response?.Data;
        }

        public async Task<List<Concept>> ListTagsByDOIAsync(string doi)
        {
            var formattedDOI = doi.Contains("doi.org") ? doi : $"https://doi.org/{doi}";
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest($"works/{formattedDOI}");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");
            request.AddQueryParameter("select", "concepts");

            var response = await client.ExecuteGetAsync<Work>(request);
            return response?.Data?.Concepts;
        }

        public async Task<Dictionary<string, List<int>>> GetAbstractFromDOIAsync(string doi)
        {
            var formattedDOI = doi.Contains("doi.org") ? doi : $"https://doi.org/{doi}";
            var client = new RestClient($"{_configuration["OpenAlex:URL"]}");
            var request = new RestRequest($"works/{formattedDOI}");
            request.AddQueryParameter("mailto", $"{_configuration["OpenAlex:Email"]}");
            request.AddQueryParameter("select", "abstract_inverted_index");

            var response = await client.ExecuteGetAsync<Work>(request);
            return response?.Data?.AbstractInvertedIndex;
        }
    }
}
