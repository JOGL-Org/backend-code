using System.ComponentModel.DataAnnotations;

namespace Jogl.Server.IdentityService.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Redirect_uri { get; set; }
    }
}
