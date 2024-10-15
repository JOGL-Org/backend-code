using System.Security.Claims;

namespace Jogl.Server.API.Services
{
    public class ContextService : IContextService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public ContextService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string CurrentUserId => _contextAccessor.HttpContext.User?.FindFirst(ClaimTypes.Sid)?.Value;
    }
}
