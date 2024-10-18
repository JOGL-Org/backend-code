using Microsoft.Extensions.Configuration;
using RestSharp;
using Microsoft.Extensions.Logging;
using System.Xml.Serialization;
using MoreLinq;
using Jogl.Server.Arxiv.DTO;

namespace Jogl.Server.Arxiv
{
    public class ArxivFacade : IArxivFacade
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ArxivFacade> _logger;

        public ArxivFacade(IConfiguration configuration, ILogger<ArxivFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<Entry>> ListPapersAsync(IEnumerable<string> categories, int page, int pageSize)
        {
            var client = new RestClient($"https://export.arxiv.org/api");
            var request = new RestRequest($"query?search_query={string.Join("+OR+", categories.Select(c => $"cat:{c}"))}&sortBy=lastUpdatedDate&sortOrder=descending&start={(page - 1) * pageSize}&max_results={pageSize}");

            var response = await client.ExecuteGetAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"ARXIV API returned {response.StatusCode}");

            XmlSerializer serializer = new XmlSerializer(typeof(Feed));
            using (StringReader reader = new StringReader(response.Content))
            {
                var data = (Feed)serializer.Deserialize(reader);
                return data.Entry;
            }
        }

        public async Task<List<Entry>> ListNewPapersAsync(DateTime since)
        {
            var res = new List<Entry>();
            foreach (var categoryBatch in ListCategories().Batch(8))
            {
                int page = 1;
                while (true)
                {
                    var papers = await ListPapersAsync(categoryBatch, page++, 100);
                    var newPapers = papers.Where(p => p.Updated > since);
                    res.AddRange(newPapers);
                    if (newPapers.Count() != papers.Count())
                        break;
                }
            }

            return res;
        }

        public List<string> ListCategories()
        {
            return File.ReadAllLines("archiv_categories.txt").ToList();
        }

    }
}