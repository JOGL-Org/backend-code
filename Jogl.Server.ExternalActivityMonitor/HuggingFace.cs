using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.HuggingFace;
using Jogl.Server.HuggingFace.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Mailer
{
    public class HuggingFace : Base<Jogl.Server.HuggingFace.DTO.Discussion>
    {
        private readonly IHuggingFaceFacade _huggingfaceFacade;
        private readonly IContentService _contentService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IContentEntityRepository _contentEntityRepository;
        private readonly IFeedIntegrationRepository _feedIntegrationRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public HuggingFace(IHuggingFaceFacade huggingfaceFacade, IContentService contentService, IFeedEntityService feedEntityService, IContentEntityRepository contentEntityRepository, IFeedIntegrationRepository feedIntegrationRepository, IConfiguration configuration, ILoggerFactory loggerFactory) : base(contentService, feedEntityService, contentEntityRepository, feedIntegrationRepository, configuration)
        {
            _huggingfaceFacade = huggingfaceFacade;
            _logger = loggerFactory.CreateLogger<HuggingFace>();
        }

        protected override FeedIntegrationType Type => FeedIntegrationType.HuggingFace;

        [Function("HuggingFace")]
        public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
        {
            await RunPRs();
        }

        protected override ContentEntity GenerateContentEntity(Jogl.Server.HuggingFace.DTO.Discussion pr, FeedIntegration integration)
        {
            return new ContentEntity
            {
                CreatedByUserId = integration.CreatedByUserId,
                CreatedUTC = DateTime.UtcNow,
                ExternalID = integration.SourceId + "-" + pr.Num.ToString(),
                ExternalSourceID = integration.Id.ToString(),
                FeedId = integration.FeedId,
                Text = $"A new PR was opened: <a href=\"https://huggingface.co/{integration.SourceId}/discussions/{pr.Num}\">{pr.Title}</a> in <a href=\"https://huggingface.co/{integration.SourceId}\">{integration.SourceId}</a>",
                Type = ContentEntityType.Announcement,
                Status = ContentEntityStatus.Active,
                Overrides = new ContentEntityOverrides
                {
                    UserAvatarURL = pr.Author?.AvatarUrl,
                    UserName = $"{pr.Author?.Name} (huggingface.co)",
                    UserURL = $"https://huggingface.co/{pr.Author?.Name}",
                }
            };
        }

        protected override string GetExternalId(Jogl.Server.HuggingFace.DTO.Discussion pr)
        {
            return pr.Num.ToString();
        }

        protected async override Task<List<Jogl.Server.HuggingFace.DTO.Discussion>> ListPRsAsync(string sourceId)
        {
            return await _huggingfaceFacade.ListPRsAsync(sourceId);
        }
    }
}
