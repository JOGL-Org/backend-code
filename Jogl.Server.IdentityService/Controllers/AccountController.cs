using Duende.IdentityServer.Services;
using Jogl.Server.Auth;
using Jogl.Server.DB;
using Jogl.Server.IdentityService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jogl.Server.IdentityService.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IProfileService _profileService;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public AccountController(IIdentityServerInteractionService interaction, IProfileService profileService, IUserRepository userRepository, IAuthService authService)
        {
            _interaction = interaction;
            _profileService = profileService;
            _userRepository = userRepository;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var vm = new LoginViewModel
            {
                Redirect_uri = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.Redirect_uri);
            if (context == null) return BadRequest("Invalid redirect URL");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            try
            {
                var user = _userRepository.Get(u => u.Email == model.Email);
                var validationResult = _authService.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt);

                if (validationResult)
                {
                    var claims = new List<Claim>
                    {
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Sid, user.Id.ToString())
                    };

                    await HttpContext.SignInAsync(new Duende.IdentityServer.IdentityServerUser(user.Id.ToString())
                    {
                        AdditionalClaims = claims,
                        DisplayName = user.Username,
                    });

                    //_logger.LogInformation("User {Username} logged in successfully", model.Username);
                    return Redirect(model.Redirect_uri);
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Login failed for user {Username}", model.Username);
                ModelState.AddModelError("", "An error occurred during login");
                return View(model);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            await HttpContext.SignOutAsync();

            if (!string.IsNullOrEmpty(logout?.PostLogoutRedirectUri))
            {
                return Redirect(logout.PostLogoutRedirectUri);
            }

            return RedirectToAction("Account", "Login");
        }
    }
}
