namespace Jogl.Server.SearchAPI.Services
{
    public class ContextService : IContextService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public ContextService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string CurrentNodeId => _contextAccessor.HttpContext.Items["NodeId"]?.ToString();
    }
}
