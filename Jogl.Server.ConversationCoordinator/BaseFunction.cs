using Jogl.Server.Conversation.Data;
using Jogl.Server.ConversationCoordinator.Services;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.ConversationCoordinator
{
    public abstract class BaseFunction
    {
        protected readonly IOutputServiceFactory _outputServiceFactory;
        protected readonly IConfiguration _configuration;

        public BaseFunction(IOutputServiceFactory outputServiceFactory, IConfiguration configuration)
        {
            _outputServiceFactory = outputServiceFactory;
            _configuration = configuration;
        }

        protected async Task<string> MirrorConversationAsync(string text, Data.User user)
        {
            var outputService = _outputServiceFactory.GetService(Const.TYPE_SLACK);
            var ids = await outputService.SendMessagesAsync(user, _configuration["Slack:Mirror:WorkspaceID"], _configuration["Slack:Mirror:ChannelID"], null, [text]);
            return ids.Single().MessageId;
        }

        protected async Task MirrorRepliesAsync(string mirrorConversationId, List<string> text, Data.User? user = null)
        {
            var outputService = _outputServiceFactory.GetService(Const.TYPE_SLACK);
            var ids = await outputService.SendMessagesAsync(user, _configuration["Slack:Mirror:WorkspaceID"], _configuration["Slack:Mirror:ChannelID"], mirrorConversationId, text);
        }

        protected async Task MirrorReplyAsync(string mirrorConversationId, string text, Data.User? user = null)
        {
            await MirrorRepliesAsync(mirrorConversationId, [text], user);
        }
    }
}
