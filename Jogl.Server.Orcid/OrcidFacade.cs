using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Jogl.Server.Orcid.DTO;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Orcid
{
    public class OrcidFacade : IOrcidFacade
    {
        private readonly HttpClient CrossRefApiClient;
        private readonly HttpClient DataCiteApiClient;

        private const string CrossrefApiBaseUrl = "https://api.crossref.org";
        private const string DataCiteApiBaseUrl = "https://api.datacite.org";
        private string AccessToken = "";

        private readonly IConfiguration _configuration;
        private readonly ILogger<OrcidFacade> _logger;

        public OrcidFacade(IConfiguration configuration, ILogger<OrcidFacade> logger)
        {
            _configuration = configuration;
            _logger = logger;

            CrossRefApiClient = new HttpClient();
            CrossRefApiClient.BaseAddress = new Uri(CrossrefApiBaseUrl);

            DataCiteApiClient = new HttpClient();
            DataCiteApiClient.BaseAddress = new Uri(DataCiteApiBaseUrl);
        }

        static XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("person", "http://www.orcid.org/ns/person");
            nsManager.AddNamespace("personal-details", "http://www.orcid.org/ns/personal-details");
            nsManager.AddNamespace("researcher-url", "http://www.orcid.org/ns/researcher-url");
            nsManager.AddNamespace("keyword", "http://www.orcid.org/ns/keyword");
            nsManager.AddNamespace("email", "http://www.orcid.org/ns/email");
            nsManager.AddNamespace("address", "http://www.orcid.org/ns/address");
            nsManager.AddNamespace("distinction", "http://www.orcid.org/ns/distinction");
            nsManager.AddNamespace("education", "http://www.orcid.org/ns/education");
            nsManager.AddNamespace("membership", "http://www.orcid.org/ns/membership");
            nsManager.AddNamespace("funding", "http://www.orcid.org/ns/funding");
            nsManager.AddNamespace("employment", "http://www.orcid.org/ns/employment");
            nsManager.AddNamespace("qualification", "http://www.orcid.org/ns/qualification");
            nsManager.AddNamespace("service", "http://www.orcid.org/ns/service");
            nsManager.AddNamespace("work", "http://www.orcid.org/ns/work");
            nsManager.AddNamespace("invited-position", "http://www.orcid.org/ns/invited-position");
            nsManager.AddNamespace("peer-review", "http://www.orcid.org/ns/peer-review");
            nsManager.AddNamespace("research-resource", "http://www.orcid.org/ns/research-resource");
            nsManager.AddNamespace("activities", "http://www.orcid.org/ns/activities");
            nsManager.AddNamespace("common", "http://www.orcid.org/ns/common");
            nsManager.AddNamespace("bulk", "http://www.orcid.org/ns/bulk");

            return nsManager;
        }

        public async Task<(string?, string?)> GetOrcidIdAsync(string authorizationCode, string redirectUrlType)
        {
            var client = new RestClient($"{_configuration["ORCID:OAuthURL"]}/token");
            var request = new RestRequest();
            //request.AddBody(new
            //{
            //    client_id = _configuration["ORCID:ClientId"],
            //    client_secret = _configuration["ORCID:ClientSecret"],
            //    grant_type = "authorization_code",
            //    code = authorizationCode,
            //    redirect_uri = _configuration["App:URL"]
            //});

            //request.AddHeader("Accept", "application/json");
            string redirectUrl;

            if (redirectUrlType == "linkOrcid")
            {
                redirectUrl = _configuration["App:URL"] + "/user/orcid";
            }
            else
            {
                redirectUrl = redirectUrlType == "signin" ? _configuration["App:URL"] + "/signin" : _configuration["App:URL"] + "/signup";
            }

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"client_id={_configuration["ORCID:ClientId"]}&client_secret={_configuration["ORCID:ClientSecret"]}&grant_type=authorization_code&code={authorizationCode}&redirect_uri={redirectUrl}", ParameterType.RequestBody);
            var res = await client.ExecutePostAsync<OrcidTokenResponse>(request);

            return (res.Data?.Orcid, res.Data?.AccessToken);
        }

        public async Task RevokeOrcidIdAsync(string accessToken)
        {
            var client = new RestClient($"{_configuration["ORCID:OAuthURL"]}/revoke");
            var request = new RestRequest();

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"client_id={_configuration["ORCID:ClientId"]}&client_secret={_configuration["ORCID:ClientSecret"]}&token={accessToken}", ParameterType.RequestBody);

            await client.ExecutePostAsync(request);
        }

        private class OrcidTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
            public string Name { get; set; }
            public string Orcid { get; set; }
        }
        public async Task<Person> GetPersonalInfo(string orcidId, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                string recordEndpoint = $"{_configuration["ORCID:URL"]}{orcidId}/person";
                string recordData = await FetchData(recordEndpoint, accessToken);
                Person person = new Person();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(recordData);

                // Extract person name
                XmlNode? nameNode = doc.SelectSingleNode("/person:person/person:name/personal-details:given-names", GetNamespaceManager(doc));
                if (nameNode != null)
                {
                    string givenNames = nameNode.InnerText;
                    person.GivenName = givenNames;
                }

                XmlNode? familyNameNode = doc.SelectSingleNode("/person:person/person:name/personal-details:family-name", GetNamespaceManager(doc));
                if (familyNameNode != null)
                {
                    string familyName = familyNameNode.InnerText;
                    person.FamilyName = familyName;
                }

                // Extract biography
                XmlNode? biographyNode = doc.SelectSingleNode("/person:person/person:biography/personal-details:content", GetNamespaceManager(doc));
                if (biographyNode != null)
                {
                    string biography = biographyNode.InnerText;
                    person.Biography = biography;
                }

                // Extract researcher URLs
                XmlNodeList? researcherUrlNodes = doc.SelectNodes("/person:person/researcher-url:researcher-urls/researcher-url:researcher-url", GetNamespaceManager(doc));
                if (researcherUrlNodes != null)
                {
                    foreach (XmlNode urlNode in researcherUrlNodes)
                    {

                        XmlNode? urlNameNode = urlNode.SelectSingleNode("researcher-url:url-name", GetNamespaceManager(doc));
                        string urlName = "";
                        if (urlNameNode != null)
                        {
                            urlName = urlNameNode.InnerText;
                        }

                        XmlNode? urlValueNode = urlNode.SelectSingleNode("researcher-url:url", GetNamespaceManager(doc));
                        string urlValue = "";
                        if (urlValueNode != null)
                        {
                            urlValue = urlValueNode.InnerText;
                        }

                        ResearcherWebsite website = new ResearcherWebsite
                        {
                            UrlName = urlName,
                            Url = urlValue
                        };

                        person.ResearcherUrls.Add(website);
                    }
                }
                // Extract keywords
                XmlNodeList? keywordNodes = doc.SelectNodes("/person:person/keyword:keywords/keyword:keyword", GetNamespaceManager(doc));
                if (keywordNodes != null)
                {
                    foreach (XmlNode keywordNode in keywordNodes)
                    {
                        XmlNode? keywordContentNode = keywordNode.SelectSingleNode("keyword:content", GetNamespaceManager(doc));
                        if (keywordContentNode != null)
                        {
                            string keyword = keywordContentNode.InnerText;
                            person.Keywords.Add(keyword);
                        }
                    }
                }

                // Extract email addresses
                XmlNodeList? emailNodes = doc.SelectNodes("/person:person/email:emails/email:email", GetNamespaceManager(doc));
                if (emailNodes != null)
                {
                    foreach (XmlNode emailNode in emailNodes)
                    {
                        string email = emailNode.InnerText;
                        person.Emails.Add(email);
                    }
                }

                // Extract addresses
                XmlNodeList? addressNodes = doc.SelectNodes("/person:person/address:addresses/address:address", GetNamespaceManager(doc));
                if (addressNodes != null)
                {
                    foreach (XmlNode addressNode in addressNodes)
                    {
                        XmlNode? countryNode = addressNode.SelectSingleNode("address:country", GetNamespaceManager(doc));
                        if (countryNode != null)
                        {
                            string country = countryNode.InnerText;
                            person.Country.Add(country);
                        }
                    }
                }

                return person;
            }

            throw new Exception("Failed to retrieve personal information.");
        }

        public async Task<List<Work>> GetWorksAsync(string orcidId, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                string recordEndpoint = $"{_configuration["ORCID:URL"]}{orcidId}/works";
                string recordData = await FetchData(recordEndpoint, accessToken);
                if (string.IsNullOrEmpty(recordData))
                    return new List<Work>();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(recordData);

                List<string> putCodes = new List<string>();

                XmlNodeList? groupNodes = doc.SelectNodes("/activities:works/activities:group", GetNamespaceManager(doc));
                if (groupNodes != null)
                {
                    foreach (XmlNode groupNode in groupNodes)
                    {
                        XmlNodeList? workNodes = groupNode.SelectNodes("work:work-summary", GetNamespaceManager(doc));
                        if (workNodes != null)
                        {
                            XmlAttributeCollection attributes = workNodes[0].Attributes;

                            foreach (XmlAttribute attribute in attributes)
                            {
                                if (attribute.Name == "put-code")
                                {
                                    putCodes.Add(attribute.Value);
                                    break;
                                }
                            }
                            // foreach (XmlNode workNode in workNodes)
                            // {

                            // }

                        }
                    }
                }

                List<Task<string>> fetchTasks = new List<Task<string>>();
                foreach (string putCode in putCodes)
                {
                    Task<string> task = FetchData($"{recordEndpoint}/{putCode}", accessToken);
                    await Task.Delay(100);
                    fetchTasks.Add(task);
                }

                // Await all the fetch tasks
                string[] results = await Task.WhenAll(fetchTasks);

                List<Work> works = new List<Work>();

                // Process the results
                foreach (string result in results)
                {
                    Work work = new Work();

                    XmlDocument workDoc = new XmlDocument();
                    workDoc.LoadXml(result);

                    XmlNode? workNode = workDoc.SelectSingleNode("/bulk:bulk/work:work", GetNamespaceManager(workDoc));

                    if (workNode != null)
                    {
                        XmlNode? titleNode = workNode.SelectSingleNode("work:title/common:title", GetNamespaceManager(workDoc));
                        work.Title = titleNode != null ? titleNode.InnerText : "";

                        XmlNode? descriptionNode = workNode.SelectSingleNode("work:short-description", GetNamespaceManager(workDoc));
                        work.Description = descriptionNode != null ? descriptionNode.InnerText : "";

                        XmlNode? workTypeNode = workNode.SelectSingleNode("work:type", GetNamespaceManager(workDoc));
                        work.WorkType = workTypeNode != null ? workTypeNode.InnerText : "";

                        XmlNode? sourceNameNode = workNode.SelectSingleNode("common:source/common:source-name", GetNamespaceManager(workDoc));
                        work.SourceName = sourceNameNode != null ? sourceNameNode.InnerText : "";

                        XmlNodeList? externalIdsNodes = workNode.SelectNodes("common:external-ids/common:external-id", GetNamespaceManager(workDoc));
                        if (externalIdsNodes != null)
                        {
                            foreach (XmlNode externalIdNode in externalIdsNodes)
                            {
                                ExternalId id = new ExternalId();
                                XmlNode? externalIdTypeNode = externalIdNode.SelectSingleNode("common:external-id-type", GetNamespaceManager(workDoc));
                                XmlNode? externalIdValueNode = externalIdNode.SelectSingleNode("common:external-id-value", GetNamespaceManager(workDoc));
                                XmlNode? externalIdUrlNode = externalIdNode.SelectSingleNode("common:external-id-url", GetNamespaceManager(workDoc));
                                XmlNode? externalIdRelationNode = externalIdNode.SelectSingleNode("common:external-id-relationship", GetNamespaceManager(workDoc));

                                if (externalIdTypeNode != null && externalIdTypeNode.InnerText == "doi")
                                {
                                    id.UrlType = externalIdTypeNode != null ? externalIdTypeNode.InnerText : "";
                                    id.UrlValue = externalIdValueNode != null ? externalIdValueNode.InnerText : "";
                                    id.Url = externalIdUrlNode != null ? externalIdUrlNode.InnerText : "";
                                    id.Relationship = externalIdRelationNode != null ? externalIdRelationNode.InnerText : "";

                                    work.ExternalIds.Add(id);
                                }
                            }
                        }

                        XmlNode? publicationDateNode = workNode.SelectSingleNode("common:publication-date", GetNamespaceManager(workDoc));
                        if (publicationDateNode != null)
                        {
                            XmlNode? yearNode = publicationDateNode.SelectSingleNode("common:year", GetNamespaceManager(workDoc));
                            XmlNode? monthNode = publicationDateNode.SelectSingleNode("common:month", GetNamespaceManager(workDoc));
                            XmlNode? dayNode = publicationDateNode.SelectSingleNode("common:day", GetNamespaceManager(workDoc));

                            string year = yearNode != null ? yearNode.InnerText : "";
                            string month = monthNode != null ? monthNode.InnerText : "";
                            string day = dayNode != null ? dayNode.InnerText : "";

                            work.PublicationDate = $"{year}{(month != "" ? "-" + month : "")}{(day != "" ? "-" + day : "")}";
                        }

                        XmlNode? journalTitleNode = workNode.SelectSingleNode("work:journal-title", GetNamespaceManager(workDoc));
                        work.JournalTitle = journalTitleNode != null ? journalTitleNode.InnerText : "";

                        XmlNodeList? contributorsNode = workNode.SelectNodes("work:contributors/work:contributor", GetNamespaceManager(workDoc));
                        if (contributorsNode != null)
                        {
                            foreach (XmlNode contributorNode in contributorsNode)
                            {
                                Contributor contributor = new Contributor();

                                XmlNode? contributorNameNode = contributorNode.SelectSingleNode("work:credit-name", GetNamespaceManager(workDoc));
                                contributor.Name = contributorNameNode != null ? contributorNameNode.InnerText : "";

                                XmlNode? contributorRoleNode = contributorNode.SelectSingleNode("work:contributor-attributes/work:contributor-role", GetNamespaceManager(workDoc));
                                contributor.Role = contributorRoleNode != null ? contributorRoleNode.InnerText : "";

                                work.Contributors.Add(contributor);
                            }
                        }
                    }

                    if (work.ExternalIds.Count > 0)
                    {
                        works.Add(work);
                    }
                }

                return works;
            }

            throw new Exception("Failed to retrieve works.");
        }

        public async Task<List<Education>> GetEducationsAsync(string orcidId, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                string recordEndpoint = $"{_configuration["ORCID:URL"]}{orcidId}/educations";
                string recordData = await FetchData(recordEndpoint, accessToken);
                if (string.IsNullOrEmpty(recordData))
                    return new List<Education>();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(recordData);

                List<Education> Educations = new List<Education>();

                XmlNodeList? groupNodes = doc.SelectNodes("/activities:educations/activities:affiliation-group", GetNamespaceManager(doc));
                if (groupNodes != null)
                {
                    foreach (XmlNode groupNode in groupNodes)
                    {
                        Education edu = new Education();
                        XmlNode? eduNode = groupNode.SelectSingleNode("education:education-summary", GetNamespaceManager(doc));

                        if (eduNode != null)
                        {
                            XmlNode? organizationNameNode = eduNode.SelectSingleNode("common:organization/common:name", GetNamespaceManager(doc));
                            edu.OrganizationName = organizationNameNode != null ? organizationNameNode.InnerText : "";

                            XmlNode? departmentNameNode = eduNode.SelectSingleNode("common:department-name", GetNamespaceManager(doc));
                            edu.DepartmentName = departmentNameNode != null ? departmentNameNode.InnerText : "";

                            XmlNode? degreeNameNode = eduNode.SelectSingleNode("common:role-title", GetNamespaceManager(doc));
                            edu.DegreeName = degreeNameNode != null ? degreeNameNode.InnerText : "";

                            XmlNode? startDateNode = eduNode.SelectSingleNode("common:start-date", GetNamespaceManager(doc));

                            if (startDateNode != null)
                            {
                                XmlNode? yearNode = startDateNode.SelectSingleNode("common:year", GetNamespaceManager(doc));
                                XmlNode? monthNode = startDateNode.SelectSingleNode("common:month", GetNamespaceManager(doc));
                                XmlNode? dayNode = startDateNode.SelectSingleNode("common:day", GetNamespaceManager(doc));

                                string year = yearNode != null ? yearNode.InnerText : "";
                                string month = monthNode != null ? monthNode.InnerText : "";
                                string day = dayNode != null ? dayNode.InnerText : "";

                                edu.StartDate = $"{year}{(month != "" ? "-" + month : "")}{(day != "" ? "-" + day : "")}";
                            }

                            XmlNode? endDateNode = eduNode.SelectSingleNode("common:end-date", GetNamespaceManager(doc));

                            if (endDateNode != null)
                            {
                                XmlNode? yearNode = endDateNode.SelectSingleNode("common:year", GetNamespaceManager(doc));
                                XmlNode? monthNode = endDateNode.SelectSingleNode("common:month", GetNamespaceManager(doc));
                                XmlNode? dayNode = endDateNode.SelectSingleNode("common:day", GetNamespaceManager(doc));

                                string year = yearNode != null ? yearNode.InnerText : "";
                                string month = monthNode != null ? monthNode.InnerText : "";
                                string day = dayNode != null ? dayNode.InnerText : "";

                                edu.EndDate = $"{year}{(month != "" ? "-" + month : "")}{(day != "" ? "-" + day : "")}";
                            }
                        }

                        Educations.Add(edu);
                    }
                }

                return Educations;
            }

            throw new Exception("Failed to retrieve educations.");
        }

        public async Task<List<Employment>> GetEmploymentsAsync(string orcidId, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                string recordEndpoint = $"{_configuration["ORCID:URL"]}{orcidId}/employments";
                string recordData = await FetchData(recordEndpoint, accessToken);
                if (string.IsNullOrEmpty(recordData))
                    return new List<Employment>();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(recordData);

                List<Employment> Employments = new List<Employment>();

                XmlNodeList? groupNodes = doc.SelectNodes("/activities:employments/activities:affiliation-group", GetNamespaceManager(doc));
                if (groupNodes != null)
                {
                    foreach (XmlNode groupNode in groupNodes)
                    {
                        Employment emp = new Employment();
                        XmlNode? empNode = groupNode.SelectSingleNode("employment:employment-summary", GetNamespaceManager(doc));

                        if (empNode != null)
                        {
                            XmlNode? organizationNameNode = empNode.SelectSingleNode("common:organization/common:name", GetNamespaceManager(doc));
                            emp.OrganizationName = organizationNameNode != null ? organizationNameNode.InnerText : "";

                            XmlNode? departmentNameNode = empNode.SelectSingleNode("common:department-name", GetNamespaceManager(doc));
                            emp.DepartmentName = departmentNameNode != null ? departmentNameNode.InnerText : "";

                            XmlNode? positionNameNode = empNode.SelectSingleNode("common:role-title", GetNamespaceManager(doc));
                            emp.PositionName = positionNameNode != null ? positionNameNode.InnerText : "";

                            XmlNode? startDateNode = empNode.SelectSingleNode("common:start-date", GetNamespaceManager(doc));

                            if (startDateNode != null)
                            {
                                XmlNode? yearNode = startDateNode.SelectSingleNode("common:year", GetNamespaceManager(doc));
                                XmlNode? monthNode = startDateNode.SelectSingleNode("common:month", GetNamespaceManager(doc));
                                XmlNode? dayNode = startDateNode.SelectSingleNode("common:day", GetNamespaceManager(doc));

                                string year = yearNode != null ? yearNode.InnerText : "";
                                string month = monthNode != null ? monthNode.InnerText : "";
                                string day = dayNode != null ? dayNode.InnerText : "";

                                emp.StartDate = $"{year}{(month != "" ? "-" + month : "")}{(day != "" ? "-" + day : "")}";
                            }

                            XmlNode? endDateNode = empNode.SelectSingleNode("common:end-date", GetNamespaceManager(doc));

                            if (endDateNode != null)
                            {
                                XmlNode? yearNode = endDateNode.SelectSingleNode("common:year", GetNamespaceManager(doc));
                                XmlNode? monthNode = endDateNode.SelectSingleNode("common:month", GetNamespaceManager(doc));
                                XmlNode? dayNode = endDateNode.SelectSingleNode("common:day", GetNamespaceManager(doc));

                                string year = yearNode != null ? yearNode.InnerText : "";
                                string month = monthNode != null ? monthNode.InnerText : "";
                                string day = dayNode != null ? dayNode.InnerText : "";

                                emp.EndDate = $"{year}{(month != "" ? "-" + month : "")}{(day != "" ? "-" + day : "")}";
                            }
                        }

                        Employments.Add(emp);
                    }
                }

                return Employments;
            }

            throw new Exception("Failed to retrieve employments.");
        }
        private async Task<string> GetAccessToken()
        {
            if (AccessToken != "")
            {
                return await Task.FromResult(AccessToken);
            }

            using (HttpClient client = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _configuration["ORCID:ClientId"] },
                { "client_secret",_configuration["ORCID:ClientSecret"] },
                { "grant_type", "client_credentials" },
                { "scope", "/read-public" }
            });

                var response = await client.PostAsync($"{_configuration["ORCID:OAuthURL"]}/token", requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic tokenResponse = JsonConvert.DeserializeObject(responseContent);
                    string accessToken = tokenResponse.access_token;
                    AccessToken = accessToken;
                    return accessToken;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<string> FetchData(string endpoint, string accessToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                var response = await client.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Orcid fetching {endpoint} failed with HTTP " + response.StatusCode + ". Full response: " + responseContent);

                return responseContent;
            }
        }

        public async Task<Work?> GetWorkFromDOI(string doi)
        {
            try
            {
                HttpResponseMessage response = await CrossRefApiClient.GetAsync($"/works/{doi}/agency");
                response.EnsureSuccessStatusCode();

                dynamic responseBody = await response.Content.ReadAsStringAsync();
                var workData = JsonConvert.DeserializeObject(responseBody).message;
                if (workData != null)
                {
                    if (workData.agency.id == "crossref")
                    {
                        Work? data = await GetWorkDataFromCrossref(doi);

                        return data;
                    }
                    else if (workData.agency.id == "datacite")
                    {
                        Work? data = await GetWorkDataFromDataCite(doi);

                        return data;
                    }
                    else
                    {
                        throw new Exception("The DOI registered agency may not offer public APIs");
                    }
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching data from Crossref: {ex.Message}");
                return null;
            }
        }

        private async Task<Work?> GetWorkDataFromCrossref(string doi)
        {
            try
            {
                HttpResponseMessage response = await CrossRefApiClient.GetAsync($"/works/{doi}");
                response.EnsureSuccessStatusCode();

                dynamic responseBody = await response.Content.ReadAsStringAsync();
                var workData = JsonConvert.DeserializeObject(responseBody).message;

                if (workData != null)
                {
                    Work work = new()
                    {
                        Title = workData.title.Count > 0 ? workData.title[0] : "",
                        JournalTitle = workData["container-title"].Count > 0 ? workData["container-title"][0] : "",

                        Description = workData.Property("abstract") != null ? workData["abstract"] : "",

                        WorkType = workData.type,
                        SourceName = "CrossRef",
                        PublicationDate = $"{workData["created"]["date-parts"][0][0]}-{workData["created"]["date-parts"][0][1]}-{workData["created"]["date-parts"][0][2]}"
                    };

                    if (workData.Property("subject") != null)
                    {
                        List<string> subjectList = new();
                        for (int i = 0; i < workData.subject.Count; i++)
                        {

                            subjectList.Add($"{workData.subject[i]}");
                        }
                        work.Tags = subjectList;
                    }

                        //for (int i = 0; i < workData.author.Count; i++)
                        //{
                        //    var author = workData.author[i];
                        //    Contributor contributor = new Contributor
                        //    {
                        //        Name = author?.name ?? (author?.given + author?.family),
                        //        Role = "Author"
                        //    };

                        //    work.Contributors.Add(contributor);
                        //}

                    ExternalId url = new()
                    {
                        Url = workData.URL,
                        UrlType = workData.URL,
                        UrlValue = workData.DOI
                    };
                    work.ExternalIds.Add(url);

                    return work;
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching data from Crossref: {ex.Message}");
                return null;
            }
        }

        private async Task<Work?> GetWorkDataFromDataCite(string doi)
        {
            try
            {
                HttpResponseMessage response = await DataCiteApiClient.GetAsync($"/dois/{doi}");
                response.EnsureSuccessStatusCode();

                dynamic responseBody = await response.Content.ReadAsStringAsync();
                Console.Write(responseBody);

                var workData = JsonConvert.DeserializeObject(responseBody).data.attributes;
                if (workData != null)
                {
                    Work work = new Work();
                    work.Title = workData.titles.Count > 0 ? workData.titles[0].title : "";
                    work.JournalTitle = workData.publisher;
                    work.Description = workData.descriptions.Count > 0 ? workData.descriptions[0].description : "";
                    work.WorkType = workData.types.resourceTypeGeneral;
                    work.SourceName = "Datacite";

                    for (int i = 0; i < workData.dates.Count; i++)
                    {
                        var dateType = workData.dates[i].dateType;

                        if (dateType == "Created")
                        {
                            work.PublicationDate = workData.dates[i].date;
                            break;
                        }
                    }

                    for (int i = 0; i < workData.creators.Count; i++)
                    {
                        var author = workData.creators[i];
                        Contributor contributor = new Contributor();
                        contributor.Name = author.name;
                        contributor.Role = "Author";

                        work.Contributors.Add(contributor);
                    }


                    List<string> subjectList = new List<string>();
                    for (int i = 0; i < workData.subjects.Count; i++)
                    {
                        subjectList.Add($"{workData.subjects[i].subject}");
                    }
                    work.Tags = subjectList;

                    ExternalId url = new ExternalId();
                    url.Url = $"https://doi.org/${workData.doi}";
                    url.UrlValue = workData.doi;
                    url.UrlType = "DOI";
                    work.ExternalIds.Add(url);

                    return work;
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching data from Datacite: {ex.Message}");
                return null;
            }
        }
    }

}