using Microsoft.AspNetCore.Mvc;
using Jogl.Server.AI;
using Jogl.Server.SlackAgentAPI.Handler;
using SlackNet.Events;
using SlackNet;
using Jogl.Server.DB;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ISlackApiClient _slackApiClient;
        private readonly IInterfaceChannelRepository _interfaceChannelRepository;
        private readonly ILogger<MessageHandler> _logger;

        public AdminController(ISlackApiClient slackApiClient, IInterfaceChannelRepository interfaceChannelRepository, ILogger<MessageHandler> logger)
        {
            _slackApiClient = slackApiClient;
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
            var users = new List<User>();
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

                if (user.Name != "filip.vostatek")
                    continue;

                var userChannelId = await client.Conversations.Open([user.Id]);

                await client.Chat.PostMessage(new SlackNet.WebApi.Message { Channel = userChannelId, Text = $"Hello {user.Profile.Email}" });
            }

            return Ok();
        }
    }
}