using Jogl.Server.Data;

namespace Jogl.Server.Business
{
    public interface IAgentService
    {
        Task NotifyAsync(ContentEntity contentEntity);
    }
}