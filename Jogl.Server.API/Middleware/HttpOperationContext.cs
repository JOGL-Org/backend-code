using Jogl.Server.DB.Context;

namespace Jogl.Server.API.Middleware
{
    public class HttpOperationContext : IOperationContext
    {
        private IHttpContextAccessor _contextAccessor;
        public HttpOperationContext(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string? UserId
        {
            get { return _contextAccessor.HttpContext.Items["UserId"].ToString(); }
            set { _contextAccessor.HttpContext.Items["UserId"] = value; }
        }

        public string? NodeId
        {
            get { return _contextAccessor.HttpContext.Items["NodeId"].ToString(); }
            set { _contextAccessor.HttpContext.Items["NodeId"] = value; }
        }
    }
}