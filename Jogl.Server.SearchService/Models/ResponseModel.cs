namespace Jogl.Server.SearchService.Models
{
    public class ResponseModel<T>
    {
        public string Text { get; set; }
        public List<T> Results { get; set; }
    }
}
