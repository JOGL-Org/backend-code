using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Jogl.Server.PubMed.DTO;
using Jogl.Server.Data.Util;
using System.Text.RegularExpressions;

namespace Jogl.Server.PubMed
{
    public class PubMedFacade : IPubMedFacade
    {
        private readonly IConfiguration _configuration;

        public PubMedFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<MeshHeading[]> GetTagsFromDOI(string doi)
        {
            var pmid = await GetPMIDFromDOI(doi);

            if (pmid != null)
            {
                var meshHeadings = await GetMeshHeadingFromPMID(pmid);

                return meshHeadings;
            }

            return null;
        }

        public async Task<string?> GetPMIDFromDOI(string doi)
        {
            var client = new RestClient($"{_configuration["PubMed:PmIdURL"]}/");
            var request = new RestRequest("/");
            request.AddQueryParameter("ids", doi);
            request.AddQueryParameter("idtype", "doi");
            request.AddQueryParameter("format", "json");
            request.AddQueryParameter("versions", "no");
            request.AddQueryParameter("showaiid", "no");
            request.AddQueryParameter("tool", "jogl");
            request.AddQueryParameter("email", "srikanth@jogl.io");

            var response = await client.ExecuteGetAsync<PMIDResponse>(request);

            if (response.Data != null && response.Data?.Records?.Count > 0)
            {
                return response.Data.Records[0].PMID;
            }

            return null;
        }

        public async Task<MeshHeading[]> GetMeshHeadingFromPMID(string pmid)
        {
            string recordData = await FetchData($"{_configuration["PubMed:EUtilsURL"]}/efetch.fcgi?db=pubmed&id={pmid}");

            var doc = new XmlDocument();
            doc.LoadXml(recordData);

            var meshHeadingNodes = doc.SelectNodes("//MeshHeading");

            var meshHeadings = new MeshHeading[meshHeadingNodes.Count];

            for (int i = 0; i < meshHeadingNodes.Count; i++)
            {
                var meshHeadingNode = meshHeadingNodes[i];
                var meshHeading = new MeshHeading
                {
                    DescriptorName = meshHeadingNode.SelectSingleNode("DescriptorName")?.InnerText,
                };

                var qualifierNodes = meshHeadingNode.SelectNodes("QualifierName");
                if (qualifierNodes != null && qualifierNodes.Count > 0)
                {
                    meshHeading.QualifierNames = new string[qualifierNodes.Count];
                    for (int j = 0; j < qualifierNodes.Count; j++)
                    {
                        meshHeading.QualifierNames[j] = qualifierNodes[j]?.InnerText;
                    }
                }

                meshHeadings[i] = meshHeading;
            }

            return meshHeadings;
        }

        private async Task<string> FetchData(string endpoint)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                var response = await client.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return responseContent;
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    Console.WriteLine("Error fetching data: " + responseContent);
                    throw new Exception("Failed to fetch data.");
                }
            }
        }

        public async Task<ListPage<PubmedArticle>> ListArticlesAsync(string search, int page, int pageSize)
        {
            var client = new RestClient($"{_configuration["PubMed:EUtilsUrl"]}");

            var eSearchRequest = new RestRequest("esearch.fcgi");
            eSearchRequest.AddQueryParameter("db", "pubmed");
            eSearchRequest.AddQueryParameter("retmode", "json");
            eSearchRequest.AddQueryParameter("retmax", pageSize);
            eSearchRequest.AddQueryParameter("retstart", (page - 1) * pageSize);
            eSearchRequest.AddQueryParameter("usehistory", "y");
            eSearchRequest.AddQueryParameter("term", GetQueryFromSearch(search));

            var eSearchResponse = await client.ExecuteGetAsync<ESearchResponse>(eSearchRequest);

            if ((eSearchResponse?.Data?.ESearchResult?.IdList?.Count ?? 0) <= 0)
            {
                return new ListPage<PubmedArticle>(new List<PubmedArticle>(), 0);
            }

            var eFetchRequest = new RestRequest("efetch.fcgi");
            eFetchRequest.AddQueryParameter("db", "pubmed");
            eFetchRequest.AddQueryParameter("WebEnv", eSearchResponse.Data.ESearchResult.WebEnv);
            eFetchRequest.AddQueryParameter("query_key", eSearchResponse.Data.ESearchResult.QueryKey);
            eFetchRequest.AddQueryParameter("id", string.Join(",", eSearchResponse.Data.ESearchResult.IdList?.ToArray()));

            //Don't define any type <T> for ExecuteGetAsync, so no time is wasted trying to parse the content and failing due to DTD errors
            var eFetchResponse = await client.ExecuteGetAsync(eFetchRequest);

            //Deserialize content
            var articleSet = Deserialize(eFetchResponse.Content);

            //Decided for try/catch instead of TryParse because if this fails, we probably want to know
            int totalResults;
            try
            {
                totalResults = int.Parse(eSearchResponse.Data.ESearchResult.Count);
            }
            catch
            {
                totalResults = eSearchResponse.Data.ESearchResult.IdList?.Count() ?? 0;
            }

            return new ListPage<PubmedArticle>(articleSet.PubmedArticles, totalResults);
        }

        public async Task<List<PubmedArticle>> ListArticlesAsync(IEnumerable<string> ids, string webenv = null, string queryKey = null)
        {
            var client = new RestClient($"{_configuration["PubMed:EUtilsUrl"]}");

            var eFetchRequest = new RestRequest("efetch.fcgi");
            eFetchRequest.AddQueryParameter("db", "pubmed");
            eFetchRequest.AddQueryParameter("id", string.Join(",", ids));
            if (string.IsNullOrEmpty(webenv))
                eFetchRequest.AddQueryParameter("WebEnv", webenv);
            if (string.IsNullOrEmpty(queryKey))
                eFetchRequest.AddQueryParameter("query_key", queryKey);

            //Don't define any type <T> for ExecuteGetAsync, so no time is wasted trying to parse the content and failing due to DTD errors
            var eFetchResponse = await client.ExecuteGetAsync(eFetchRequest);

            //Deserialize content
            var articleSet = Deserialize(eFetchResponse.Content);
            return articleSet.PubmedArticles;
        }

        public async Task<List<PubmedArticle>> ListNewPapersAsync(string lastId)
        {
            //var date = DateTime.Today;
            // var term = "\"(\\\"2024/11/01\\\"[Date - Create] : \\\"2024/11/01\\\"[Date - Create])\"";
            var res = new List<PubmedArticle>();

            while (true)
            {
                var idNumber = int.Parse(lastId);
                var idRange = Enumerable.Range(idNumber + 1, 20).Select(n => n.ToString());

                var page = await ListArticlesAsync(idRange);
                if (page.Count == 0)
                    return res;

                lastId = idRange.Last();
                res.AddRange(page);

                //avoid PubMed API throttling
                Thread.Sleep(500);
            }
        }

        private string GetQueryFromSearch(string search)
        {
            return search?.Replace(" ", " ")?.ToLower();
        }

        private PubmedArticleSet Deserialize(string xml)
        {
            try
            {

                PubmedArticleSet articleSet;
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Parse
                };

                string escapedXML =
                    xml.Replace("<i>", "").Replace("</i>", "")
                       .Replace("<sub>", "").Replace("</sub>", "")
                       .Replace("<sup>", "").Replace("</sup>", "")
                       .Replace("<u>", "").Replace("</u>", "")
                       .Replace("<b>", "").Replace("</b>", "");

                escapedXML = Regex.Replace(escapedXML, @"<(mml:\w+)[^>]*>.*?</\1>", "");

                using (var stringReader = new StringReader(escapedXML))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    var serializer = new XmlSerializer(typeof(PubmedArticleSet));
                    articleSet = (PubmedArticleSet)serializer.Deserialize(xmlReader);
                }
                return articleSet;
            }
            catch (Exception ex)
            {
                return new PubmedArticleSet { PubmedArticles = new List<PubmedArticle>() };
            }

        }

        public List<string> ListCategories()
        {
            return Resources.pubmed_categories.Split(Environment.NewLine).Select(cat => cat.Split("|")[0]).ToList();
        }
    }
}