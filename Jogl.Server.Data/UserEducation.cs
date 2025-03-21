namespace Jogl.Server.Data
{
    public class UserEducation
    {
        public string School { get; set; }
        public string? Program { get; set; }
        public string? Description { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public bool Current { get; set; }
    }
}