using Jogl.Server.Data.Util;

namespace Jogl.Server.Data
{
    public class FAQItem
    {
        public string Question { get; set; }
        [RichText]
        public string Answer { get; set; }
    }
}