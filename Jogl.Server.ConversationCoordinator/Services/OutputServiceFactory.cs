using Jogl.Server.Conversation.Data;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class OutputServiceFactory(ISlackOutputService slackOutputService) : IOutputServiceFactory
    {
        public IOutputService GetService(string type)
        {
            switch (type)
            {
                case Const.TYPE_SLACK:
                    return slackOutputService;
                default:
                    return null;
            }
        }
    }
}
