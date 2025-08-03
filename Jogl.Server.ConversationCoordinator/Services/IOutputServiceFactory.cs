namespace Jogl.Server.ConversationCoordinator.Services
{
    public interface IOutputServiceFactory
    {
        IOutputService GetService(string type);
    }
}
