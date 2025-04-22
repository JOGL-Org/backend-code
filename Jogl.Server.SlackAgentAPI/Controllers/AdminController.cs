using Microsoft.AspNetCore.Mvc;
using Jogl.Server.SlackAgentAPI.Handler;
using Jogl.Server.DB;
using Jogl.Server.Business;
using Jogl.Server.URL;
using Jogl.Server.Data;
using Jogl.Server.Slack;
using Jogl.Server.Conversation.Data;
using Jogl.Server.Notifications;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ISlackService _slackService;
        private readonly INotificationFacade _notificationFacade;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly ILogger<MessageHandler> _logger;

        public AdminController(ISlackService slackService, INotificationFacade notificationFacade, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceUserRepository interfaceUserRepository, ILogger<MessageHandler> logger)
        {
            _slackService = slackService;
            _notificationFacade = notificationFacade;
            _interfaceChannelRepository = interfaceChannelRepository;
            _logger = logger;
        }

        [HttpGet]
        [Route("greet")]
        public async Task<IActionResult> GreetChannel()
        {
            var channel = _interfaceChannelRepository.Get("67fd7b0ab84f14d25d8214d0");
            if (channel == null)
                return NotFound();

            if (string.IsNullOrEmpty(channel.Key))
            {
                _logger.LogWarning("Channel not initialized with access key {0}", channel.ExternalId);
                return BadRequest();
            }

            if (string.IsNullOrEmpty(channel.NodeId))
            {
                _logger.LogWarning("Slack workspace missing a node id: {0}", channel.ExternalId);
                return BadRequest();
            }

            var users = await _slackService.ListWorkspaceUsersAsync(channel.Key);
            foreach (var user in users)
            {
                if (user.Profile.Email != "thomas+99@JOGL.io")
                    continue;

                await _notificationFacade.NotifyAsync(Const.USER_JOINED_INTERFACE_CHANNEL, new UserJoined
                {
                    ChannelExternalId = user.TeamId,
                    User = new Conversation.Data.User
                    {
                        Email = user.Profile.Email,
                        FirstName = user.Profile.FirstName,
                        LastName = user.Profile.LastName,
                        ExternalId = user.Id,
                    }
                });
            }

            return Ok();
        }
    }
}