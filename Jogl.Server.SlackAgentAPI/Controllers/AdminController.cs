using Microsoft.AspNetCore.Mvc;
using Jogl.Server.SlackAgentAPI.Handler;
using Jogl.Server.DB;
using Jogl.Server.Business;
using Jogl.Server.URL;
using Jogl.Server.Data;
using Jogl.Server.Slack;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ISlackService _slackService;
        private readonly IUserService _userService;
        private readonly IUrlService _urlService;
        private readonly IMembershipService _membershipService;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly IInterfaceUserRepository _interfaceUserRepository;
        private readonly ILogger<MessageHandler> _logger;

        public AdminController(ISlackService slackService, IUserService userService, IUrlService urlService, IMembershipService membershipService, IInterfaceChannelRepository interfaceChannelRepository, IInterfaceUserRepository interfaceUserRepository, ILogger<MessageHandler> logger)
        {
            _slackService = slackService;
            _userService = userService;
            _urlService = urlService;
            _membershipService = membershipService;
            _interfaceChannelRepository = interfaceChannelRepository;
            _interfaceUserRepository = interfaceUserRepository;
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

                var code = await _userService.GetOnetimeLoginCodeAsync(user.Profile.Email);
                var url = _urlService.GetOneTimeLoginLink(user.Profile.Email, code);
                var channelId = await _slackService.GetUserChannelIdAsync(channel.Key, user.Id);

                var existingUser = _userService.GetForEmail(user.Profile.Email);
                if (existingUser != null)
                {
                    var existingInterfaceUser = _interfaceUserRepository.Get(iu => iu.UserId == existingUser.Id.ToString());
                    if (existingInterfaceUser == null)
                        await _interfaceUserRepository.CreateAsync(new InterfaceUser
                        {
                            CreatedByUserId = existingUser.Id.ToString(),
                            CreatedUTC = DateTime.UtcNow,
                            UserId = existingUser.Id.ToString(),
                            ChannelId = channel.Id.ToString(),
                            ExternalId = user.Id,
                        });

                    await _slackService.SendMessageAsync(channel.Key, channelId, $"Hello {user.Profile.FirstName}, it seems you already have a JOGL profile! You can log in <{url}|here>");
                }
                else
                {
                    var userId = await _userService.CreateAsync(new User
                    {
                        FirstName = user.Profile.FirstName,
                        LastName = user.Profile.LastName,
                        Email = user.Profile.Email,
                        Status = UserStatus.Verified,
                        CreatedUTC = DateTime.UtcNow,
                    });

                    await _interfaceUserRepository.CreateAsync(new InterfaceUser
                    {
                        CreatedByUserId = userId,
                        CreatedUTC = DateTime.UtcNow,
                        UserId = userId,
                        ChannelId = channel.Id.ToString(),
                        ExternalId = user.Id,
                    });

                    await _membershipService.AddMembersAsync([new Membership {
                        CreatedByUserId = userId,
                        CreatedUTC = DateTime.UtcNow,
                        CommunityEntityId = channel.NodeId,
                        CommunityEntityType = CommunityEntityType.Node,
                        AccessLevel = AccessLevel.Member,
                        UserId = userId,
                    }]);

                    await _slackService.SendMessageAsync(channel.Key, channelId, $"Hello {user.Profile.FirstName}, welcome to JOGL! We've set up your JOGL account on {user.Profile.Email}. You can set up your profile <{url}|here>");
                }
            }

            return Ok();
        }
    }
}