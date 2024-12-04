using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Auth;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Orcid;
using Jogl.Server.GoogleAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.LinkedIn;
using Jogl.Server.API.Services;
using Jogl.Server.GitHub;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IUserVerificationService _userVerificationService;
        private readonly IAuthService _authService;
        private readonly IAuthChallengeService _challengeService;
        private readonly IGoogleFacade _googleFacade;
        private readonly ILinkedInFacade _linkedInFacade;
        private readonly IGitHubFacade _githubFacade;
        private readonly IInvitationService _invitationService;
        private readonly IConfiguration _configuration;
        private readonly IOrcidFacade _orcidFacade;

        public AuthController(IGoogleFacade googleFacade, ILinkedInFacade linkedInFacade, IGitHubFacade githubFacade, IOrcidFacade orcidFacade, IUserService userService, IUserVerificationService userVerificationService, IAuthService authService, IAuthChallengeService authChallengeService, IInvitationService invitationService, IConfiguration configuration, IMapper mapper, ILogger<AuthController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _userService = userService;
            _userVerificationService = userVerificationService;
            _authService = authService;
            _challengeService = authChallengeService;
            _invitationService = invitationService;
            _configuration = configuration;
            _orcidFacade = orcidFacade;
            _googleFacade = googleFacade;
            _linkedInFacade = linkedInFacade;
            _githubFacade = githubFacade;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [Route("login/password")]
        [SwaggerOperation($"Logs a user in using an email and password")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid user credentials")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "User not yet verified")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User login successful", typeof(AuthResultModel))]
        public async Task<IActionResult> Login([FromBody] UserLoginPasswordModel model)
        {
            var user = _userService.GetForEmail(model.Email);
            if (user == null)
                return Unauthorized();

            var userToken = _authService.GetTokenWithPassword(user, model.Password);
            if (string.IsNullOrEmpty(userToken))
                return Unauthorized();

            if (user.Status == Data.UserStatus.Pending)
                return Forbid();

            return Ok(new AuthResultModel
            {
                Token = userToken,
                UserId = user.Id.ToString()
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("verification")]
        [SwaggerOperation($"Creates a new email verification for a given email")]
        [SwaggerResponse((int)HttpStatusCode.OK, "No user found for email")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The verification was created")]
        public async Task<IActionResult> VerificationStart(VerificationStartModel model)
        {
            var user = _userService.GetForEmail(model.Email);
            if (user == null)
                return NotFound();

            await _userVerificationService.CreateAsync(user, VerificationAction.Verify, null, true);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("wallet/challenge/{wallet}")]
        [SwaggerOperation($"Retrieves a user challenge for a wallet signature login")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User challenge", typeof(string))]
        public async Task<IActionResult> GetLoginChallenge([FromRoute] string wallet)
        {
            var challenge = _challengeService.GetChallenge(wallet);
            return Ok(challenge);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("wallet/{walletType}")]
        [SwaggerOperation($"Logs a user in using a web3 wallet signature")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid signature")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "User not yet verified")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User login successful", typeof(AuthResultModel))]
        public async Task<IActionResult> LoginWithWalletSignature([FromRoute] WalletType walletType, [FromBody] UserLoginSignatureModel model)
        {
            var user = _userService.GetForWallet(model.Wallet);
            if (user == null)
                return Unauthorized();

            var userToken = _authService.GetTokenWithSignature(user, walletType, model.Wallet, model.Signature);
            if (string.IsNullOrEmpty(userToken))
                return Unauthorized();

            if (user.Status == Data.UserStatus.Pending)
                return Forbid();

            return Ok(new AuthResultModel
            {
                Token = userToken,
                UserId = user.Id.ToString()
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("verification/check")]
        [SwaggerOperation($"Returns the status of a user email verification for a given email and code")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The verification status", typeof(VerificationStatus))]
        public async Task<IActionResult> GetVerificationStatus(string email, string code)
        {
            var status = _userVerificationService.GetVerificationStatus(email, VerificationAction.Verify, code);
            return Ok(status);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("verification/confirm")]
        [SwaggerOperation($"Confirms the verification for a given email and code")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The verification was completed")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The supplied verification code is invalid")]
        public async Task<IActionResult> VerificationConfirm(VerificationConfirmationModel model)
        {
            var status = await _userVerificationService.VerifyAsync(model.Email, VerificationAction.Verify, model.Code);
            switch (status.Status)
            {
                case VerificationStatus.Invalid:
                    return BadRequest();
                default:
                    return Ok();
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("reset")]
        [SwaggerOperation($"Initiates the password reset process using the supplied email address")]
        [SwaggerResponse((int)HttpStatusCode.OK, "")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var redirectUrl = $"{_configuration["App:URL"]}/auth/new-password";
            await _userService.ResetPasswordAsync(email, redirectUrl);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("resetConfirm")]
        [SwaggerOperation($"Resets a user's password using a code delivered to their email address")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user's password was reset successfully")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not exist, the code is invalid or has expired")]
        public async Task<IActionResult> ForgotPasswordConfirm(UserForgotPasswordModel model)
        {
            var res = await _userService.ResetPasswordConfirmAsync(model.Email, model.Code, model.NewPassword);
            if (!res)
                return Forbid();

            return Ok();
        }

        [HttpPost]
        [Route("password")]
        [SwaggerOperation($"Updates a user's password")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user's password was reset successfully")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The old password is invalid")]
        public async Task<IActionResult> SetPassword([FromBody] UserPasswordChangeModel model)
        {
            var user = _userService.Get(CurrentUserId);
            var passwordOk = _authService.VerifyPassword(model.OldPassword, user.PasswordHash, user.PasswordSalt);
            if (!passwordOk)
                return Forbid();

            await _userService.SetPasswordAsync(user.Id.ToString(), model.NewPassword);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("code/trigger")]
        [SwaggerOperation($"Initiates the one-time login process using the supplied email address")]
        public async Task<IActionResult> InitiateLoginWithCode(string email)
        {
            var redirectUrl = $"{_configuration["App:URL"]}/signin/onetime";
            await _userService.OneTimeLoginAsync(email, redirectUrl);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("code/login")]
        [SwaggerOperation($"Verifies the one-time login code and completes the one-time login processs. If a user doesn't exist with the supplied email, it will be created. A user created in this manner has no password and can only log in via further one-time codes until they link another authentication method to their account.")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid user credentials")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User login successful")]
        public async Task<IActionResult> LoginWithCode([FromBody] UserLoginCodeModel model)
        {
            var ok = await _userService.VerifyOneTimeLoginAsync(model.Email, model.Code);
            if (!ok)
                return Unauthorized();

            var userId = _userService.GetForEmail(model.Email)?.Id.ToString();
            if (string.IsNullOrEmpty(userId))
                userId = await _userService.CreateAsync(new Data.User
                {
                    Email = model.Email,
                    Status = Data.UserStatus.Verified
                });

            return Ok(new AuthResultModel
            {
                Token = _authService.GetToken(model.Email),
                UserId = userId
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("orcid")]
        [SwaggerOperation($"register or sign-in the user's unique ORCID id from ORCID and stores it in the database")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "No ORCID record found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Login or registration forbidden")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Login or registration successful", typeof(AuthResultExtendedModel))]
        public async Task<IActionResult> LoginOrSignupWithOrcid([FromBody] OrcidRegistrationModel model)
        {
            var (orcid, accessToken) = await _orcidFacade.GetOrcidIdAsync(model.AuthorizationCode, model.Screen);
            if (string.IsNullOrEmpty(orcid) || string.IsNullOrEmpty(accessToken))
                return Unauthorized();

            var data = await _orcidFacade.GetPersonalInfo(orcid, accessToken);
            if (data.Emails.Count == 0)
                return BadRequest("Email not found in ORCID data.");

            var existingUser = _userService.GetForEmail(data.Emails[0], true);
            if (existingUser == null)
            {
                var user = new User
                {
                    FirstName = data.GivenName.Trim(),
                    LastName = data.FamilyName?.Trim() ?? string.Empty,
                    Username = _userService.GetUniqueUsername(data.GivenName, data.FamilyName),
                    Email = data.Emails[0].Trim(),
                    OrcidId = orcid,
                    Auth = new UserExternalAuth
                    {
                        OrcidAccessToken = accessToken,
                        IsOrcidUser = true
                    },
                    Status = UserStatus.Verified,
                };

                await InitCreationAsync(user);
                var userId = await _userService.CreateAsync(user);

                return Ok(new AuthResultExtendedModel
                {
                    Token = _authService.GetToken(data.Emails[0]),
                    UserId = userId,
                    Created = true
                });
            }

            if (existingUser.Deleted)
            {
                return Forbid();
            }

            existingUser.OrcidId = orcid;
            existingUser.Auth = new UserExternalAuth
            {
                OrcidAccessToken = accessToken
            };

            await _userService.UpdateAsync(existingUser);

            return Ok(new AuthResultExtendedModel
            {
                Token = _authService.GetToken(existingUser.Email),
                UserId = existingUser.Id.ToString(),
                Created = false
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("google")]
        [SwaggerOperation($"Register or sign-in the user with a google access token")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "No google record found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Login or registration forbidden")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Login or registration successful", typeof(AuthResultExtendedModel))]
        public async Task<IActionResult> LoginOrSignupWithGoogle([FromBody] AccessTokenModel model)
        {
            var profile = await _googleFacade.GetUserInfoAsync(model.AccessToken);
            if (profile == null)
                return Unauthorized();

            var existingUser = _userService.GetForEmail(profile.email, true);
            if (existingUser == null)
            {
                var user = new User
                {
                    FirstName = profile.GivenName.Trim(),
                    LastName = profile.LastName?.Trim() ?? string.Empty,
                    Username = _userService.GetUniqueUsername(profile.GivenName, profile.LastName),
                    Email = profile.email.Trim(),
                    Auth = new UserExternalAuth
                    {
                        IsGoogleUser = true
                    },
                    Status = UserStatus.Verified
                };

                await InitCreationAsync(user);
                var userId = await _userService.CreateAsync(user);

                return Ok(new AuthResultExtendedModel
                {
                    Token = _authService.GetToken(profile.email),
                    UserId = userId,
                    Created = true
                });
            }

            if (existingUser.Deleted)
            {
                return Forbid();
            }

            return Ok(new AuthResultExtendedModel
            {
                Token = _authService.GetToken(profile.email),
                UserId = existingUser.Id.ToString(),
                Created = false
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("linkedin")]
        [SwaggerOperation($"Register or sign-in the user with a linkedin access token")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "No linkedin record found")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Login or registration forbidden")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Login or registration successful", typeof(AuthResultExtendedModel))]
        public async Task<IActionResult> LoginOrSignupWithLinkedIn([FromBody] AuthorizationCodeModel model)
        {
            var token = await _linkedInFacade.GetAccessTokenAsync(model.AuthorizationCode);
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var profile = await _linkedInFacade.GetUserInfoAsync(token);
            if (profile == null)
                return Unauthorized();

            var existingUser = _userService.GetForEmail(profile.email, true);
            if (existingUser == null)
            {
                var user = new User
                {
                    FirstName = profile.GivenName.Trim(),
                    LastName = profile.LastName?.Trim() ?? string.Empty,
                    Username = _userService.GetUniqueUsername(profile.GivenName, profile.LastName),
                    Email = profile.email.Trim(),
                    Auth = new UserExternalAuth
                    {
                        IsLinkedInUser = true
                    },
                    Status = UserStatus.Verified
                };

                await InitCreationAsync(user);
                var userId = await _userService.CreateAsync(user);

                return Ok(new AuthResultExtendedModel
                {
                    Token = _authService.GetToken(profile.email),
                    UserId = userId,
                    Created = true
                });
            }

            if (existingUser.Deleted)
            {
                return Forbid();
            }

            return Ok(new AuthResultExtendedModel
            {
                Token = _authService.GetToken(profile.email),
                UserId = existingUser.Id.ToString(),
                Created = false
            });
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("github")]
        //[SwaggerOperation($"Register or sign-in the user with a github access token")]
        //[SwaggerResponse((int)HttpStatusCode.NotFound, "No github record found or user not found")]
        //public async Task<IActionResult> LoginOrSignupWithGithub([FromBody] AuthorizationCodeModel model)
        //{
        //    var token = await _githubFacade.GetAccessTokenAsync(model.AuthorizationCode);
        //    if (string.IsNullOrEmpty(token))
        //        return Unauthorized();

        //    var profile = await _githubFacade.GetUserInfoAsync(token);
        //    if (profile == null)
        //        return Unauthorized();

        //    var existingUser = _userService.GetForEmail(profile.email);
        //    if (existingUser == null)
        //    {
        //        var user = new User
        //        {
        //            FirstName = profile.GivenName.Trim(),
        //            LastName = profile.LastName?.Trim() ?? string.Empty,
        //            Username = _userService.GetUniqueUsername(profile.GivenName, profile.LastName),
        //            Email = profile.email.Trim(),
        //            Auth = new UserExternalAuth
        //            {
        //                IsLinkedInUser = true
        //            },
        //            Status = UserStatus.Verified
        //        };

        //        await InitCreationAsync(user);
        //        var userId = await _userService.CreateAsync(user);

        //        return Ok(new
        //        {
        //            token = _authService.GetToken(profile.email),
        //            userId,
        //            created = true
        //        });
        //    }

        //    return Ok(new
        //    {
        //        token = _authService.GetToken(profile.email),
        //        userId = existingUser.Id.ToString(),
        //        created = false
        //    });
        //}
    }
}