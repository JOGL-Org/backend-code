using Jogl.Server.Data.Util;

namespace Jogl.Server.Data
{
    public class OnboardingRules
    {
        public bool Enabled { get; set; }
        [RichText]
        public string Text { get; set; }
    }
}