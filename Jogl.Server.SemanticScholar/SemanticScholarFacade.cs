using Jogl.Server.Data.Util;
using Jogl.Server.SemanticScholar.DTO;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Jogl.Server.SemanticScholar
{
    public class SemanticScholarFacade : ISemanticScholarFacade
    {
        private readonly IConfiguration _configuration;

        public SemanticScholarFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ListPage<SemanticPaper>> ListWorksAsync(string search, int page, int pageSize)
        {
            var client = new RestClient($"{_configuration["SemanticScholar:URL"]}");
            var request = new RestRequest("paper/search");
            request.AddHeader("x-api-key", _configuration["SemanticScholar:ApiKey"]);
            request.AddQueryParameter("offset", (page - 1) * pageSize);
            request.AddQueryParameter("limit", pageSize);
            request.AddQueryParameter("query", GetQueryFromSearch(search));
            request.AddQueryParameter("fields", "externalIds,publicationDate,publicationTypes,openAccessPdf,paperId,abstract,url,authors,citationCount,title,journal,year");

            var response = await client.ExecuteGetAsync<Response<SemanticPaper>>(request);
            return new ListPage<SemanticPaper>(response.Data.Data ?? new List<SemanticPaper>(), response.Data.Total);
        }

        public async Task<SemanticPaper> GetWorkAsync(string id)
        {
            var client = new RestClient($"{_configuration["SemanticScholar:URL"]}");
            var request = new RestRequest($"paper/{id}");
            request.AddHeader("x-api-key", _configuration["SemanticScholar:ApiKey"]);
            request.AddQueryParameter("fields", "externalIds,publicationDate,publicationTypes,openAccessPdf,paperId,abstract,url,authors,citationCount,title,journal,year");

            var response = await client.ExecuteGetAsync<SemanticPaper>(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return response.Data;
        }

        public async Task<ListPage<Author>> ListAuthorsAsync(string search, int page, int pageSize)
        {
            var client = new RestClient($"{_configuration["SemanticScholar:URL"]}");
            var request = new RestRequest("author/search");
            request.AddHeader("x-api-key", _configuration["SemanticScholar:ApiKey"]);
            request.AddQueryParameter("offset", (page - 1) * pageSize);
            request.AddQueryParameter("limit", pageSize);
            request.AddQueryParameter("query", GetQueryFromSearch(search));
            request.AddQueryParameter("fields", "name,papers.externalIds,papers.publicationDate,papers.publicationTypes,papers.openAccessPdf,papers.paperId,papers.abstract,papers.url,papers.authors,papers.citationCount,papers.title,papers.journal,papers.year");

            var response = await client.ExecuteGetAsync<Response<Author>>(request);
            return new ListPage<Author>(response.Data.Data, response.Data.Total);
        }

        public async Task<SemanticTags> ListTagsByDOIAsync(string search)
        {
            var client = new RestClient($"{_configuration["SemanticScholar:URL"]}");
            var request = new RestRequest($"paper/DOI:{search}");
            request.AddHeader("x-api-key", _configuration["SemanticScholar:ApiKey"]);
            request.AddQueryParameter("fields", "title,s2FieldsOfStudy");

            var response = await client.ExecuteGetAsync<SemanticTags>(request);
            return response.Data;
        }

        private string GetQueryFromSearch(string search)
        {
            return search?.Replace(" ", " ")?.ToLower();
        }
    }
}