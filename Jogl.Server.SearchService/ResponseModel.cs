namespace Jogl.Server.API.Controllers
{
    public class ResponseModel<T>
    {
        public string Text { get; set; }
        public List<T> Results { get; set; }
    }
}
