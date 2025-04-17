using Microsoft.AspNetCore.Mvc;
using Jogl.Server.SlackAgentAPI.Handler;
using SlackNet;
using Jogl.Server.DB;
using Jogl.Server.Business;
using Jogl.Server.URL;
using Jogl.Server.Data;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ISlackApiClient _slackApiClient;
        private readonly IUserService _userService;
        private readonly IUrlService _urlService;
        private readonly IMembershipService _membershipService;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly ILogger<MessageHandler> _logger;

        public AdminController(ISlackApiClient slackApiClient, IUserService userService, IUrlService urlService, IMembershipService membershipService, IInterfaceChannelRepository interfaceChannelRepository, ILogger<MessageHandler> logger)
        {
            _slackApiClient = slackApiClient;
            _userService = userService;
            _urlService = urlService;
            _membershipService = membershipService;
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

            if (channel.Key == null)
                return BadRequest();

            var client = _slackApiClient.WithAccessToken(channel.Key);

            var cursor = string.Empty;
            var users = new List<SlackNet.User>();
            while (true)
            {
                var page = await client.Users.List(cursor: cursor, limit: 10);
                users.AddRange(page.Members);
                cursor = page.ResponseMetadata.NextCursor;
                if (string.IsNullOrEmpty(cursor))
                    break;
            }

            foreach (var user in users)
            {
                if (user.IsBot || user.Id == "USLACKBOT")
                    continue;

                if (user.Name != "filip.vostatek" && user.Name != "louis_1214")
                    continue;

                var userChannelId = await client.Conversations.Open([user.Id]);

                var userExists = _userService.GetForEmail(user.Profile.Email) != null;
                if (userExists)
                {
                    var code = await _userService.GetOnetimeLoginCodeAsync(user.Profile.Email);
                    var url = _urlService.GetOneTimeLoginLink(user.Profile.Email, code);
                    await client.Chat.PostMessage(new SlackNet.WebApi.Message { Channel = userChannelId, Text = $"Hello {user.Profile.FirstName}, it seems you already have a JOGL profile! You can log in <{url}|here>" });
                }
                else
                {
                    var userId = await _userService.ImportUserAsync(user.Profile.FirstName, user.Profile.LastName, user.Profile.Email);

                    var code = await _userService.GetOnetimeLoginCodeAsync(user.Profile.Email);
                    var url = _urlService.GetOneTimeLoginLink(user.Profile.Email, code);
                    await _membershipService.AddMembersAsync([new Membership { CommunityEntityId = channel.NodeId, CommunityEntityType = CommunityEntityType.Node, AccessLevel = AccessLevel.Member, UserId = userId }]);
                    await client.Chat.PostMessage(new SlackNet.WebApi.Message { Channel = userChannelId, Text = $"Hello {user.Profile.FirstName}, welcome to JOGL! We've set up your JOGL account on {user.Profile.Email}. You can set up your profile <{url}|here>" });
                }
            }

            return Ok();
        }

    }
}