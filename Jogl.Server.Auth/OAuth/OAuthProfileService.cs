using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Jogl.Server.DB;
using System.Security.Claims;

namespace Jogl.Server.Auth.OAuth
{
    public class OAuthProfileService : IProfileService
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public OAuthProfileService(IAuthService authService, IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var userId = context.Subject.FindFirst("sub")?.Value;
            if (userId == null) return;

            var user = _userRepository.Get(userId);
            if (user == null) return;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Sid, user.Id.ToString())
            };

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
        }
    }
}
