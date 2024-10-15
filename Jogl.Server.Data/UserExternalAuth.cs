using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public class UserExternalAuth
    {
        public string OrcidAccessToken { get; set; }

        public bool IsOrcidUser { get; set; }
        public bool IsGoogleUser { get; set; }
        public bool IsLinkedInUser { get; set; }
    }
}