using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.GitHub;
using Jogl.Server.GitHub.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Jogl.Server.Mailer
{
    public class GitHubPRMonitor : BasePRMonitor<PullRequest>
    {
        private readonly IGitHubFacade _githubFacade;
        private readonly IContentService _contentService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public GitHubPRMonitor(IGitHubFacade githubFacade, IContentService contentService, IFeedEntityService feedEntityService, IContentEntityRepository contentEntityRepository, IFeedIntegrationRepository feedIntegrationRepository, IConfiguration configuration, ILoggerFactory loggerFactory) : base(contentService, feedEntityService, contentEntityRepository, feedIntegrationRepository, configuration)
        {
            _githubFacade = githubFacade;
            _logger = loggerFactory.CreateLogger<GitHubPRMonitor>();
        }

        protected override FeedIntegrationType Type => FeedIntegrationType.GitHub;

        [Function("GitHub-PR-monitor")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            await RunPRs();
        }

        protected override ContentEntity GenerateContentEntity(PullRequest pr, FeedIntegration integration)
        {
            return new ContentEntity
            {
                CreatedByUserId = integration.CreatedByUserId,
                CreatedUTC = DateTime.UtcNow,
                ExternalID = pr.Id.ToString(),
                ExternalSourceID = integration.Id.ToString(),
                FeedId = integration.FeedId,
                Text = $"A new PR was opened: <a href=\"{pr.HtmlUrl}\">{pr.Title}</a> in <a href=\"{pr.Base.Repo.HtmlUrl}\">{pr.Base.Repo.FullName}</a>",
                Type = ContentEntityType.Announcement,
                Status = ContentEntityStatus.Active,
                Overrides = new ContentEntityOverrides
                {
                    UserAvatarURL = pr.User?.AvatarUrl,
                    UserName = $"{pr.User?.Login} (github.com)",
                    UserURL = pr.User?.HtmlUrl
                }
            };
        }

        protected override string GetExternalId(PullRequest pr)
        {
            return pr.Id.ToString();
        }

        protected async override Task<List<PullRequest>> ListPRsAsync(FeedIntegration integration)
        {
            return await _githubFacade.ListPRsAsync(integration.SourceId, integration.AccessToken);
        }
    }
}
