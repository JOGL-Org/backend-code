using System.Text.Json.Serialization;

namespace Jogl.Server.Data
{
    public class UserContact
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string Phone { get; set; }

        public string Organization { get; set; }

        public string OrganizationSize { get; set; }

        public string Message { get; set; }
        
        public string Reason { get; set; }
        
        public string Country { get; set; }
    }
}