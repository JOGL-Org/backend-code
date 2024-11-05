using Jogl.Server.Arxiv;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.GitHub.DTO;
using Jogl.Server.Orcid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;

namespace Jogl.Server.ExternalActivityMonitor
{
    public class ArxivPublicationMonitor
    {
        private readonly IArxivFacade _arxivFacade;
        private readonly IContentService _contentService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly IConfiguration _configuration;

        public ArxivPublicationMonitor(IArxivFacade arxivFacade, IContentService contentService, IFeedEntityService feedEntityService, IPublicationRepository publicationRepository, IContentEntityRepository contentEntityRepository, IFeedIntegrationRepository feedIntegrationRepository, IConfiguration configuration)
        {
            _arxivFacade = arxivFacade;
            _contentService = contentService;
            _feedEntityService = feedEntityService;
            _publicationRepository = publicationRepository;
            _contentEntityRepository = contentEntityRepository;
            _feedIntegrationRepository = feedIntegrationRepository;
            _configuration = configuration;
        }

        [Function("ARXIV-publication-monitor")]
        public async Task Run([TimerTrigger("0 0 7 * * *")] TimerInfo myTimer)
        {
            //load new papers from arxiv
            var entries = await _arxivFacade.ListNewPapersAsync(DateTime.UtcNow.AddDays(-2));

            //transform to publication object
            var publications = entries.Select(entry =>
            {
                var id = entry.Id.Replace("http://arxiv.org/abs/", string.Empty);
                var idWithoutVersion = id.Substring(0, id.IndexOf("v"));

                return new Publication
                {
                    Authors = entry.Author.Select(a => a.Name).ToList(),
                    CreatedUTC = DateTime.UtcNow,
                    DOI = entry.Doi?.Text,
                    Journal = entry.JournalRef?.Text,
                    //LicenseURL = entry.
                    Published = entry.Published,
                    ExternalID = idWithoutVersion,
                    //Submitter =entry.
                    ExternalSystem = "ARXIV",
                    ExternalURL = $"https://arxiv.org/abs/{idWithoutVersion}",
                    ExternalFileURL = $"https://arxiv.org/pdf/{idWithoutVersion}",
                    Summary = entry.Summary,
                    Tags = entry.Category.Select(c => c.Term).ToList(),
                    Title = entry.Title
                };
            });

            //store papers
            await Parallel.ForEachAsync(publications, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (publication, cancellationToken) =>
            {
                await _publicationRepository.UpsertAsync(publication, p => p.ExternalID);
            });

            //create posts for feed integration sources
            var sources = _feedIntegrationRepository.List(eas => eas.Type == FeedIntegrationType.Arxiv && !string.IsNullOrEmpty(eas.SourceId) && !eas.Deleted);
            foreach (var source in sources)
            {
                var publicationsForSource = publications.Where(p => p.Tags.Contains(source.SourceId));
                foreach (var publication in publicationsForSource)
                {
                    var existingCe = _contentEntityRepository.Get(ce => ce.ExternalID == publication.ExternalID && ce.FeedId == source.FeedId && !ce.Deleted);
                    if (existingCe != null)
                        continue;

                    var ce = GenerateContentEntity(publication, source);
                    await _contentEntityRepository.CreateAsync(ce);

                    //mark feed entity as updated
                    await _feedEntityService.UpdateActivityAsync(source.FeedId, DateTime.UtcNow, source.CreatedByUserId);

                    //mark feed integration activity
                    source.LastActivityUTC = DateTime.UtcNow;
                    source.UpdatedByUserId = source.CreatedByUserId;
                    await _feedIntegrationRepository.UpdateLastActivityAsync(source);
                }
            }
        }

        protected ContentEntity GenerateContentEntity(Publication publication, FeedIntegration integration)
        {
            return new ContentEntity
            {
                CreatedByUserId = integration.CreatedByUserId,
                CreatedUTC = DateTime.UtcNow,
                ExternalID = publication.ExternalID,
                ExternalSourceID = integration.Id.ToString(),
                FeedId = integration.FeedId,
                Text = $"A new publication was submitted to Arxiv: <a href=\"{publication.ExternalURL}\">{publication.Title}</a>",
                Type = ContentEntityType.Announcement,
                Status = ContentEntityStatus.Active,
                Overrides = new ContentEntityOverrides
                {
                    UserAvatarURL = _configuration["App:URL"] + "/images/discussionApps/arxiv-logomark-small.svg",
                    UserName = $"Arxiv.org",
                    UserURL = publication.ExternalURL
                }
            };
        }
    }
}