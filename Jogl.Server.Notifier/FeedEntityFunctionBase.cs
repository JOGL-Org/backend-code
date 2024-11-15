using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.Email;
using Jogl.Server.PushNotifications;
using Jogl.Server.URL;
using Microsoft.Extensions.Logging;

namespace Jogl.Server.Notifier
{
    public abstract class FeedEntityFunctionBase<T> : NotificationFunctionBase where T : FeedEntity
    {
        protected readonly ICommunityEntityService _communityEntityService;
        protected readonly IEmailRecordRepository _emailRecordRepository;
        protected readonly IMembershipRepository _membershipRepository;
        protected readonly IFeedEntityService _feedEntityService;

        protected FeedEntityFunctionBase(ICommunityEntityService communityEntityService, IEmailRecordRepository emailRecordRepository, IMembershipRepository membershipRepository, IFeedEntityService feedEntityService, IUserRepository userRepository, IPushNotificationTokenRepository pushNotificationTokenRepository, IEmailService emailService, IPushNotificationService pushNotificationService, IUrlService urlService, ILogger<NotificationFunctionBase> logger) : base(userRepository, pushNotificationTokenRepository, emailService, pushNotificationService, urlService, logger)
        {
            _communityEntityService = communityEntityService;
            _emailRecordRepository = emailRecordRepository;
            _membershipRepository = membershipRepository;
            _feedEntityService = feedEntityService;
        }

        protected List<User> GetUsersForFeedEntity(T entity)
        {
            var ids = new List<string>();
            if (entity.UserVisibility != null)
                ids.AddRange(entity.UserVisibility.Select(uv => uv.UserId));

            if (entity.CommunityEntityVisibility != null)
            {
                var memberships = _membershipRepository.List(m => entity.CommunityEntityVisibility.Select(cev => cev.CommunityEntityId).Contains(m.CommunityEntityId) && !m.Deleted);
                ids.AddRange(memberships.Select(u => u.UserId));
            }

            ids.Remove(entity.UpdatedByUserId ?? entity.CreatedByUserId);
            return _userRepository.Get(ids);
        }

        protected object GetEmailPayload(T entity, FeedEntity feedEntity, User creator)
        {
            return new
            {
                NAME = creator.FeedTitle,
                ENTITY_TYPE = _feedEntityService.GetPrintName(entity.FeedType),
                ENTITY_URL = _urlService.GetUrl(entity),
                ENTITY_NAME = entity.FeedTitle,
                CONTAINER_TYPE = _feedEntityService.GetPrintName(feedEntity.FeedType),
                CONTAINER_URL = _urlService.GetUrl(feedEntity),
                CONTAINER_NAME = feedEntity.FeedTitle,
                CTA_URL = _urlService.GetUrl(entity),
            };
        }
    }
}
