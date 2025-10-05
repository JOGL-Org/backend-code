using Jogl.Server.Conversation.Data;

namespace Jogl.Server.ConversationCoordinator.Services
{
    public class OutputServiceFactory(ISlackOutputService slackOutputService, IWhatsAppOutputService whatsAppOutputService, IJOGLOutputService joglOutputService) : IOutputServiceFactory
    {
        public IOutputService GetService(string type)
        {
            switch (type)
            {
                case Const.TYPE_SLACK:
                    return slackOutputService;
                case Const.TYPE_WHATSAPP:
                    return whatsAppOutputService;
                case Const.TYPE_JOGL:
                    return joglOutputService;
                default:
                    return null;
            }
        }
    }
}
