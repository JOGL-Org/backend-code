using Azure.Messaging.ServiceBus;
using Jogl.Server.Arxiv;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Jogl.Server.ExternalActivityMonitor
{
    public class PublicationFeedProcessor
    {
        private readonly IArxivFacade _arxivFacade;
        private readonly IContentService _contentService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly IConfiguration _configuration;

        public PublicationFeedProcessor(IArxivFacade arxivFacade, IContentService contentService, IFeedEntityService feedEntityService, IPublicationRepository publicationRepository, IContentEntityRepository contentEntityRepository, IFeedIntegrationRepository feedIntegrationRepository, IConfiguration configuration)
        {
            _arxivFacade = arxivFacade;
            _contentService = contentService;
            _feedEntityService = feedEntityService;
            _publicationRepository = publicationRepository;
            _contentEntityRepository = contentEntityRepository;
            _feedIntegrationRepository = feedIntegrationRepository;
            _configuration = configuration;
        }

        [Function("publication-feed-processor")]
        public async Task RunCommentsAsync(
        [ServiceBusTrigger("publication-loaded", "feed", Connection = "ConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
        {
            var publication = JsonSerializer.Deserialize<Publication>(message.Body.ToString());
            var sources = _feedIntegrationRepository.List(eas => (eas.Type == FeedIntegrationType.Arxiv || eas.Type == FeedIntegrationType.PubMed) && publication.Tags.Contains(eas.SourceId) && !eas.Deleted);
            foreach (var source in sources)
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

            await messageActions.CompleteMessageAsync(message);
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
                Text = $"A new publication was submitted to {publication.ExternalSystem}: <a href=\"{publication.ExternalURL}\">{publication.Title}</a>",
                Type = ContentEntityType.Announcement,
                Status = ContentEntityStatus.Active,
                Overrides = GetOverrides(publication, integration)
            };
        }

        ContentEntityOverrides GetOverrides(Publication publication, FeedIntegration integration)
        {
            switch (integration.Type)
            {
                case FeedIntegrationType.Arxiv:
                    return new ContentEntityOverrides
                    {
                        UserAvatarURL = _configuration["App:URL"] + "/images/discussionApps/arxiv-logomark-small.svg",
                        UserName = $"Arxiv",
                        UserURL = publication.ExternalURL
                    };
                case FeedIntegrationType.PubMed:
                    return new ContentEntityOverrides
                    {
                        UserAvatarURL = _configuration["App:URL"] + "/images/discussionApps/US-NLM-PubMed-Logo.svg",
                        UserName = $"PubMed",
                        UserURL = publication.ExternalURL
                    };
                default:
                    return null;
            }
        }
    }
}
