using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Mailer
{
    public abstract class Base<T>
    {
        private readonly IContentService _contentService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly IConfiguration _configuration;

        public Base(IContentService contentService, IFeedEntityService feedEntityService, IContentEntityRepository contentEntityRepository, IFeedIntegrationRepository feedIntegrationRepository, IConfiguration configuration)
        {
            _contentService = contentService;
            _feedEntityService = feedEntityService;
            _contentEntityRepository = contentEntityRepository;
            _feedIntegrationRepository = feedIntegrationRepository;
            _configuration = configuration;
        }

        public async Task RunPRs()
        {
            var sources = _feedIntegrationRepository.List(eas => eas.Type == Type && !string.IsNullOrEmpty(eas.SourceId) && !eas.Deleted);
            var repos = sources.Select(gh => gh.SourceId).Distinct().ToList();

            foreach (var githubRepo in repos)
            {
                var PRs = await ListPRsAsync(githubRepo);
                foreach (var source in sources.Where(s => githubRepo.Equals(s.SourceId, StringComparison.InvariantCultureIgnoreCase)))
                {
                    foreach (var pr in PRs)
                    {
                        var existingCe = _contentEntityRepository.Get(ce => ce.ExternalID == GetExternalId(pr) && ce.FeedId == source.FeedId && !ce.Deleted);
                        if (existingCe != null)
                            continue;

                        var ce = GenerateContentEntity(pr, source);
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
        }

        protected abstract FeedIntegrationType Type { get; }

        protected abstract string GetExternalId(T pr);
        protected abstract Task<List<T>> ListPRsAsync(string sourceId);
        protected abstract ContentEntity GenerateContentEntity(T pr, FeedIntegration integration);
    }
}
