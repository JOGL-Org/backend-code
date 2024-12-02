using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Notifications;
using Jogl.Server.PubMed;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.ExternalActivityMonitor
{
    public class PubMedPublicationLoader
    {
        private readonly IPubMedFacade _pubmedFacade;
        private readonly INotificationFacade _notificationFacade;
        private readonly ISystemValueRepository _systemValueRepository;
        private readonly IConfiguration _configuration;

        public PubMedPublicationLoader(IPubMedFacade pubmedFacade, INotificationFacade notificationFacade, ISystemValueRepository systemValueRepository, IConfiguration configuration)
        {
            _pubmedFacade = pubmedFacade;
            _notificationFacade = notificationFacade;
            _systemValueRepository = systemValueRepository;
            _configuration = configuration;
        }

        [Function("PubMed-publication-loader")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            var key = "LAST_PUBMED_ID";
            var lastIdSysValue = _systemValueRepository.Get(v => v.Key == key);
            if (lastIdSysValue == null)
                throw new Exception($"Missing {key}");

            var lastId = lastIdSysValue.Value;
            var entries = await _pubmedFacade.ListNewPapersAsync(lastId);
            var publications = entries.Where(e => e.MedlineCitation?.Article != null).Select(entry =>
            {
                return new Publication
                {
                    Authors = entry.MedlineCitation.Article.AuthorList?.Author?.Select(a => a.ForeName + " " + a.LastName)?.ToList() ?? new List<string>(),
                    CreatedUTC = DateTime.UtcNow,
                    DOI = entry.PubmedData?.ArticleIdList?.ArticleId?.FirstOrDefault(aid => aid.IdType == "doi")?.Text,
                    Journal = entry.MedlineCitation.Article.Journal?.Title,
                    Published = entry.MedlineCitation.DateCompleted?.Value,
                    ExternalID = entry.MedlineCitation.PMID.Text,
                    ExternalSystem = "PUBMED",
                    ExternalURL = $"https://pubmed.ncbi.nlm.nih.gov/{entry.MedlineCitation.PMID.Text}",
                    //ExternalFileURL = $"https://arxiv.org/pdf/{idWithoutVersion}",
                    Summary = entry.MedlineCitation.Article.Abstract?.AbstractText?.FirstOrDefault()?.Text,
                    Tags = entry.MedlineCitation.MeshHeadingList?.MeshHeading?.Select(m => m.DescriptorName.Text)?.ToList() ?? new List<string>(),
                    Title = entry.MedlineCitation.Article.ArticleTitle.Text,
                };
            }).ToList();


            await _notificationFacade.NotifyLoadedAsync(publications);
            await _systemValueRepository.UpsertAsync(new SystemValue { Key = key, Value = publications.Last().ExternalID }, v => v.Key);
        }
    }
}