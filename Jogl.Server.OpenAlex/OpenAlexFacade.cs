using Jogl.Server.Data.Util;
using Jogl.Server.OpenAlex.DTO;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Jogl.Server.OpenAlex
{
    public class OpenAlexFacade : IOpenAlexFacade
    {
        private readonly IConfiguration _configuration;

        public OpenAlexFacade(IConfiguration configuration)
        {
            _configuration = configuration;
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
            return new ListPage<Work>(response?.Data?.Results ?? new List<Work>(), response.Data.Meta.Count);
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
