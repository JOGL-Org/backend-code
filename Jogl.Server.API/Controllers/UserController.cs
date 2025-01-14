using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.API.Services;
using Jogl.Server.Auth;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.Email;
using Jogl.Server.GitHub;
using Jogl.Server.HuggingFace;
using Jogl.Server.LinkedIn;
using Jogl.Server.Lix;
using Jogl.Server.OpenAlex;
using Jogl.Server.Orcid;
using Jogl.Server.PubMed;
using Jogl.Server.SemanticScholar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Jogl.Server.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IFeedEntityService _feedEntityService;
        private readonly IUserVerificationService _userVerificationService;
        private readonly IProposalService _proposalService;
        private readonly IWorkspaceService _workspaceService;
        private readonly INodeService _nodeService;
        private readonly IOrganizationService _organizationService;
        private readonly INeedService _needService;
        private readonly IInvitationService _invitationService;
        private readonly IContentService _contentService;
        private readonly ICommunityEntityService _communityEntityService;
        private readonly ITagService _tagService;
        private readonly IEventService _eventService;
        private readonly IPaperService _paperService;
        private readonly IDocumentService _documentService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IOpenAlexFacade _openAlexFacade;
        private readonly IOrcidFacade _orcidFacade;
        private readonly IPubMedFacade _pubMedFacade;
        private readonly IVerificationService _verificationService;
        private readonly IAuthService _authService;
        private readonly ISemanticScholarFacade _s2Facade;
        private readonly IGitHubFacade _githubFacade;
        private readonly IHuggingFaceFacade _huggingFaceFacade;
        private readonly ILixFacade _lixFacade;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IFeedEntityService feedEntityService, IUserVerificationService userVerificationService, IProposalService proposalService, IWorkspaceService workspaceService, INodeService nodeService, IOrganizationService organizationService, INeedService needService, IInvitationService invitationService, IContentService contentService, ICommunityEntityService communityEntityService, ITagService tagService, IEventService eventService, IPaperService paperService, IDocumentService documentService, INotificationService notificationService, IEmailService emailService, IOpenAlexFacade openAlexFacade, IOrcidFacade orcidFacade, ISemanticScholarFacade s2Facade, IPubMedFacade pubMedFacade, IAuthService authService, IConfiguration configuration, IMapper mapper, ILogger<UserController> logger, IVerificationService verificationService, IEntityService entityService, IContextService contextService, IGitHubFacade gitHubFacade, IHuggingFaceFacade huggingFaceFacade, ILixFacade lixFacade) : base(entityService, contextService, mapper, logger)
        {
            _userService = userService;
            _feedEntityService = feedEntityService;
            _userVerificationService = userVerificationService;
            _proposalService = proposalService;
            _workspaceService = workspaceService;
            _needService = needService;
            _nodeService = nodeService;
            _organizationService = organizationService;
            _invitationService = invitationService;
            _contentService = contentService;
            _communityEntityService = communityEntityService;
            _tagService = tagService;
            _eventService = eventService;
            _paperService = paperService;
            _documentService = documentService;
            _notificationService = notificationService;
            _emailService = emailService;
            _openAlexFacade = openAlexFacade;
            _orcidFacade = orcidFacade;
            _pubMedFacade = pubMedFacade;
            _verificationService = verificationService;
            _authService = authService;
            _s2Facade = s2Facade;
            _githubFacade = gitHubFacade;
            _huggingFaceFacade = huggingFaceFacade;
            _lixFacade = lixFacade;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost]
        [SwaggerOperation($"Create a new user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "ID of the new user record", typeof(string))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The supplied verification code is invalid")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The captcha code is invalid")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A user with this email or nickname already exists", typeof(ErrorModel))]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateModel model)
        {
            //check captcha
            var verificationResult = await _verificationService.VerifyAsync(model.CaptchaVerificationToken, "registration");
            if (!verificationResult)
                return Forbid();

            //check for duplicate emails
            var existingUserEmail = _userService.GetForEmail(model.Email, true);
            if (existingUserEmail != null)
                return Conflict(new ErrorModel("email"));

            //create user object
            var user = _mapper.Map<User>(model);

            //autogenerate username
            user.Username = _userService.GetUniqueUsername(user.FirstName, user.LastName);

            //create
            await InitCreationAsync(user);
            var userId = await _userService.CreateAsync(user, model.Password);

            //if no verification code present, trigger verification
            if (string.IsNullOrEmpty(model.VerificationCode))
            {
                await _userVerificationService.CreateAsync(user, VerificationAction.Verify, model.RedirectURL, true);
                return Ok(userId);
            }

            //if verification code present, process
            var result = await _userVerificationService.VerifyAsync(model.Email, VerificationAction.Verify, model.VerificationCode);
            switch (result.Status)
            {
                //if invalid, return HTTP 400
                case VerificationStatus.Invalid:
                    return BadRequest();
                //if successful, return OK
                default:
                    return Ok(userId);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("wallet/{walletType}")]
        [SwaggerOperation($"Register a user with a wallet signature")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Signature invalid")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A user with this wallet, email or nickname already exists", typeof(ErrorModel))]
        [SwaggerResponse((int)HttpStatusCode.OK, "Registration successful", typeof(string))]
        public async Task<IActionResult> CreateUserWithWallet([FromRoute] WalletType walletType, [FromBody] UserCreateWalletModel model)
        {
            if (!_authService.VerifySignature(walletType, model.Wallet, model.Signature))
                return Unauthorized();

            //check for duplicate email
            var existingUserEmail = _userService.GetForEmail(model.Email, true);
            if (existingUserEmail != null)
                return Conflict(new ErrorModel("email"));

            //check for duplicate wallet
            var existingUserForWallet = _userService.GetForWallet(model.Wallet, true);
            if (existingUserForWallet != null)
                return Conflict(new ErrorModel("wallet"));

            //create user object
            var user = new User
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Username = _userService.GetUniqueUsername(model.FirstName, model.LastName),
                Email = model.Email.Trim(),
                Status = UserStatus.Pending,
                Wallets = new List<Wallet>(new Wallet[] { new Wallet { Address = model.Wallet, Type = walletType } })
            };

            //create
            await InitCreationAsync(user);
            var userId = await _userService.CreateAsync(user);

            //trigger verification
            await _userVerificationService.CreateAsync(user, VerificationAction.Verify, null, true);
            return Ok(userId);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpPost]
        [Route("check")]
        [SwaggerOperation($"Verifies whether an email address is eligible to join JOGL")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The email address can be used to sign up with a new user")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A user with this email already exists")]
        public async Task<IActionResult> CheckEmail([FromBody] EmailModel model)
        {
            var existingUserEmail = _userService.GetForEmail(model.Email, true);
            if (existingUserEmail != null)
                return Conflict();

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user data", typeof(UserModel))]
        public async Task<IActionResult> GetUser([FromRoute] string id)
        {
            var user = _userService.GetDetail(id, CurrentUserId);
            if (user == null)
                return NotFound();

            if (user.Status != UserStatus.Verified && id != CurrentUserId)
                return NotFound();

            if (user.Id.ToString() != CurrentUserId)
                user.Email = null;

            var userModel = _mapper.Map<UserModel>(user);
            return Ok(userModel);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/portfolio")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Portfolio data", typeof(List<PortfolioItemModel>))]
        public async Task<IActionResult> GetPortfolio([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var data = _documentService.ListPortfolioForUser(CurrentUserId, id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var models = data.Select(d => _mapper.Map<PortfolioItemModel>(d));
            return Ok(models);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/needs")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        public async Task<IActionResult> GetNeeds([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var needs = _needService.ListForUser(CurrentUserId, id, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var needModels = needs.Select(_mapper.Map<NeedModel>).ToList();
            return Ok(needModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/ecosystem")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [Obsolete]
        public async Task<IActionResult> GetEcosystem([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var communities = _workspaceService.ListForUser(CurrentUserId, id, null, model.Search, model.Page, model.PageSize);
            //var nodes = _nodeService.ListEcosystemCommunities(CurrentUserId, id, model.Search, model.Page, model.PageSize);

            return Ok(new EcosystemModel
            {
                Communities = communities.Select(_mapper.Map<CommunityEntityMiniModel>).ToList(),
                //Nodes = nodes.Select(_mapper.Map<CommunityEntityMiniModel>).ToList(),
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/workspaces")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user's workspaces", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetWorkspaces([FromRoute] string id, [FromQuery] Permission? permission, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var communities = _workspaceService.ListForUser(CurrentUserId, id, permission, model.Search, model.Page, model.PageSize);
            var communityModels = communities.Select(_mapper.Map<CommunityEntityMiniModel>).ToList();
            return Ok(communityModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/nodes")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user's nodes", typeof(List<CommunityEntityMiniModel>))]
        public async Task<IActionResult> GetNodes([FromRoute] string id, [FromQuery] Permission? permission, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var nodes = _nodeService.ListForUser(CurrentUserId, id, permission, model.Search, model.Page, model.PageSize);
            var nodeModels = nodes.Select(_mapper.Map<CommunityEntityMiniModel>).ToList();
            return Ok(nodeModels);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/organizations")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        public async Task<IActionResult> GetOrganizations([FromRoute] string id, [FromQuery] Permission? permission, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var organizations = _organizationService.ListForUser(CurrentUserId, id, permission, model.Search, model.Page, model.PageSize);
            var organizationModels = organizations.Select(_mapper.Map<CommunityEntityMiniModel>).ToList();
            return Ok(organizationModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation("List all users for a given query")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of user models", typeof(ListPage<UserMiniModel>))]
        public async Task<IActionResult> Search([FromQuery] SearchModel model)
        {
            var entities = _userService.List(CurrentUserId, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var models = entities.Items.Select(_mapper.Map<UserMiniModel>);
            return Ok(new ListPage<UserMiniModel>(models, entities.Total));
        }

        [HttpPatch]
        [Route("{id}")]
        [SwaggerOperation($"Patches the specified user.")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user can only edit its own user record")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The username or email is already taken")]
        public async Task<IActionResult> PatchUser([FromRoute] string id, [FromBody] UserPatchModel model)
        {
            if (!string.Equals(CurrentUserId, id, StringComparison.InvariantCultureIgnoreCase))
                return Forbid();

            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var userModel = _mapper.Map<UserUpdateModel>(user);
            ApplyPatchModel(model, userModel);

            var updatedUser = _mapper.Map<User>(userModel);
            var existingUserForUsername = _userService.GetForUsername(updatedUser.Username);
            if (existingUserForUsername != null && existingUserForUsername.Id.ToString() != id)
                return Conflict("Username");

            var existingUserForEmail = _userService.GetForEmail(updatedUser.Email);
            if (existingUserForEmail != null && existingUserForEmail.Id.ToString() != id)
                return Conflict("Email");

            await InitUpdateAsync(updatedUser);
            await _userService.UpdateAsync(updatedUser);
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerOperation($"Updates the specified user record")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The current user can only edit its own user record")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The username or email is already taken")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UserUpdateModel model)
        {
            if (!string.Equals(CurrentUserId, id, StringComparison.InvariantCultureIgnoreCase))
                return Forbid();

            model.Id = id;
            var updatedUser = _mapper.Map<User>(model);
            var existingUserForUsername = _userService.GetForUsername(model.Username);
            if (existingUserForUsername != null && existingUserForUsername.Id.ToString() != id)
                return Conflict("Username");

            var existingUserForEmail = _userService.GetForEmail(model.Email);
            if (existingUserForEmail != null && existingUserForEmail.Id.ToString() != id)
                return Conflict("Email");

            await InitUpdateAsync(updatedUser);
            await _userService.UpdateAsync(updatedUser);
            return Ok();
        }

        [HttpPost]
        [Route("archive")]
        [SwaggerOperation($"Archives the current user")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The user cannot be archived")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The user is already archived")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user was archived successfully")]
        public async Task<IActionResult> ArchiveCurrentUser()
        {
            var user = _userService.Get(CurrentUserId);
            if (user.Status == UserStatus.Pending)
                return BadRequest();

            if (user.Status == UserStatus.Archived)
                return Conflict();

            await InitUpdateAsync(user);
            await _userService.SetArchivedAsync(user);
            return Ok();
        }

        [HttpPost]
        [Route("unarchive")]
        [SwaggerOperation($"Unarchives the current user")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The user isn't archived")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user was unarchived successfully")]
        public async Task<IActionResult> UnarchiveCurrentUser()
        {
            var user = _userService.Get(CurrentUserId);
            if (user.Status != UserStatus.Archived)
                return BadRequest();

            user.Status = UserStatus.Verified;
            await InitUpdateAsync(user);
            await _userService.SetActiveAsync(user);
            return Ok();
        }

        [HttpDelete]
        [SwaggerOperation($"Deletes the current user")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The user cannot be deleted")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The user is already archived")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The user was deleted successfully")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var user = _userService.Get(CurrentUserId);

            await InitUpdateAsync(user);
            await _userService.DeleteAsync(user);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("exists/username/{username}")]
        [SwaggerOperation($"Checks whether a username is already taken or not")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "Username is already taken")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Username is available")]
        public async Task<IActionResult> ExistsUsername([FromRoute] string username)
        {
            var user = _userService.GetForUsername(username);
            if (user == null)
                return Ok();

            return Conflict();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("exists/wallet/{wallet}")]
        [SwaggerOperation($"Checks whether a wallet address exists on JOGL or not")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Whether or not the specified wallet exits", typeof(bool))]
        public async Task<IActionResult> ExistsWallet([FromRoute] string wallet)
        {
            var user = _userService.GetForWallet(wallet);
            return Ok(user != null);
        }

        [Obsolete]
        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/invites")]
        [SwaggerOperation("List all pending community entity invitations for a given user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of community entity invitations", typeof(List<InvitationModelEntity>))]
        public async Task<IActionResult> GetInvitations([SwaggerParameter("ID of the user")][FromRoute] string id)
        {
            var invitations = _invitationService.ListForUser(id, InvitationType.Invitation);
            var invitationModels = invitations.Select(_mapper.Map<InvitationModelEntity>);
            return Ok(invitationModels);
        }

        [HttpGet]
        [Route("invites/communityEntity")]
        [SwaggerOperation("List all pending community entity invitations for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of community entity invitations", typeof(List<InvitationModelEntity>))]
        public async Task<IActionResult> GetCommunityEntityInvitations()
        {
            var invitations = _invitationService.ListForUser(CurrentUserId, InvitationType.Invitation);
            var invitationModels = invitations.Select(_mapper.Map<InvitationModelEntity>);
            return Ok(invitationModels);
        }

        [HttpGet]
        [Route("invites/event")]
        [SwaggerOperation("List all pending event invitations for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of event invitations", typeof(List<EventAttendanceModel>))]
        public async Task<IActionResult> GetEventInvitations()
        {
            var attendances = _eventService.GetAttendancesForUser(CurrentUserId);
            var attendanceModels = attendances.Select(_mapper.Map<EventAttendanceModel>);
            return Ok(attendanceModels);
        }

        [HttpPost]
        [Route("{id}/contact")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user has not enabled the Contact Me function")]
        public async Task<IActionResult> Contact([FromRoute] string id, [FromBody] ContactModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            if (!user.ContactMe)
                return Forbid();

            var appUrl = $"{_configuration["App:URL"]}/";
            await _userService.SendMessageAsync(CurrentUserId, id, appUrl, model.Subject, model.Text);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/follow")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "The current user is already following this user")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "The user cannot follow themselves")]
        public async Task<IActionResult> Follow([FromRoute] string id)
        {
            if (id == CurrentUserId)
                return BadRequest();

            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var following = _userService.GetFollowing(CurrentUserId, id);
            if (following != null)
                return Conflict();

            following = new UserFollowing { UserIdFrom = CurrentUserId, UserIdTo = id };
            await InitCreationAsync(following);
            await _userService.CreateFollowingAsync(following);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}/follow")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id or the current user isn't following this user")]
        public async Task<IActionResult> Unfollow([FromRoute] string id)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var following = _userService.GetFollowing(CurrentUserId, id);
            if (following == null)
                return NotFound();

            await _userService.DeleteFollowingAsync(following.Id.ToString());
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/followed")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        public async Task<IActionResult> GetFollowed([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var followings = _userService.GetFollowed(id, CurrentUserId, model.Search, model.Page, model.PageSize, true);
            var followingModels = followings.Select(_mapper.Map<UserMiniModel>).ToList();
            return Ok(followingModels);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/followers")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        public async Task<IActionResult> GetFollowers([FromRoute] string id, [FromQuery] SearchModel model)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var followings = _userService.GetFollowers(id, CurrentUserId, model.Search, model.Page, model.PageSize, true);
            var followingModels = followings.Select(_mapper.Map<UserMiniModel>).ToList();
            return Ok(followingModels);
        }

        [HttpPost]
        [Route("skills")]
        public async Task<IActionResult> CreateSkill([FromBody] TextValueModel model)
        {
            var skill = _userService.GetSkill(model.Value);
            if (skill != null)
                return Conflict();

            skill = new TextValue { Value = model.Value };
            await InitCreationAsync(skill);
            await _userService.CreateSkillAsync(skill);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("skills")]
        public async Task<IActionResult> GetSkills([FromQuery] SearchModel model)
        {
            var skills = _userService.GetSkills(model.Search, model.Page, model.PageSize);
            var skillModels = skills.Select(_mapper.Map<TextValueModel>);
            return Ok(skillModels);
        }

        [HttpPost]
        [Route("interests")]
        public async Task<IActionResult> CreateInterest([FromBody] TextValueModel model)
        {
            var interest = _tagService.GetTag(model.Value);
            if (interest != null)
                return Conflict();

            interest = new Tag { Text = model.Value };
            await InitCreationAsync(interest);
            await _tagService.CreateTagAsync(interest);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("interests")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The interest data", typeof(List<TextValueModel>))]
        public async Task<IActionResult> GetInterests([FromQuery] SearchModel model)
        {
            var skills = _tagService.GetTags(model.Search, model.Page, model.PageSize);
            var skillModels = skills.Select(_mapper.Map<TextValueModel>);
            return Ok(skillModels);
        }

        [Obsolete]
        [HttpPost]
        [Route("papers")]
        [SwaggerOperation($"Adds a new paper")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Note-typed papers have to be created through the feed controller")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The ID of the new paper", typeof(string))]
        public async Task<IActionResult> AddPaper([FromBody] PaperUpsertModel model)
        {
            if (model.Type == PaperType.Note)
                return BadRequest();

            var paper = _mapper.Map<Paper>(model);
            paper.FeedId = CurrentUserId;
            await InitCreationAsync(paper);
            var paperId = await _paperService.CreateAsync(paper);

            return Ok(paperId);
        }

        [HttpPost]
        [Route("orcid")]
        [SwaggerOperation($"Loads the user's unique ORCID id from ORCID and stores it in the database")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No ORCID record found or user not found")]
        public async Task<IActionResult> LoadOrcid([FromBody] OrcidLoadModel model)
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            var (orcid, accessToken) = await _orcidFacade.GetOrcidIdAsync(model.AuthorizationCode, "linkOrcid");

            if (string.IsNullOrEmpty(orcid) || string.IsNullOrEmpty(accessToken))
                return NotFound();

            user.OrcidId = orcid;
            user.Auth = new UserExternalAuth
            {
                OrcidAccessToken = accessToken
            };

            await _userService.UpdateAsync(user);

            return Ok();
        }

        [HttpDelete]
        [Route("orcid")]
        [SwaggerOperation($"Removes the user's ORCID id and all orcid-loaded papers from the database")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No ORCID record found or user not found")]
        public async Task<IActionResult> UnloadOrcid()
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            await _orcidFacade.RevokeOrcidIdAsync(user.Auth.OrcidAccessToken);

            user.OrcidId = null;
            user.Auth.OrcidAccessToken = null;

            await _userService.UpdateAsync(user);
            await _paperService.DeleteForExternalSystemAndUserAsync(CurrentUserId, ExternalSystem.ORCID);

            return Ok();
        }

        [HttpDelete]
        [Route("google")]
        [SwaggerOperation($"Removes the Google account link")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No Google record found or user not found")]
        public async Task<IActionResult> UnloadGoogle()
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            user.Auth.IsGoogleUser = false;
            await _userService.UpdateAsync(user);

            return Ok();
        }

        [HttpGet]
        [Route("papers/orcid")]
        [SwaggerOperation($"Gets papers for the current user's ORCID")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No ORCID set on the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The papers from ORCID", typeof(List<PaperModelOrcid>))]
        public async Task<IActionResult> GetOrcidPapers()
        {
            var user = _userService.Get(CurrentUserId);

            if (string.IsNullOrEmpty(user.Auth.OrcidAccessToken))
            {
                user.Auth = new UserExternalAuth
                {
                    OrcidAccessToken = null
                };

                await _userService.UpdateAsync(user);
            }

            if (string.IsNullOrEmpty(user?.OrcidId) || string.IsNullOrEmpty(user.Auth.OrcidAccessToken))
                return NotFound();

            var works = await _orcidFacade.GetWorksAsync(user.OrcidId, user.Auth.OrcidAccessToken);
            var paperModels = works.Select(_mapper.Map<PaperModelOrcid>);

            return Ok(paperModels);
        }

        [HttpGet]
        [Route("papers/s2/paper")]
        [SwaggerOperation($"Gets papers from Semantic Scholar - search papers")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The papers from S2", typeof(ListPage<PaperModelS2>))]
        public async Task<IActionResult> GetS2PapersPaper([FromQuery] ExternalSearchModel model)
        {
            var works = await _s2Facade.ListWorksAsync(model.Search, model.Page, model.PageSize);
            var paperModels = works.Items.Select(_mapper.Map<PaperModelS2>).ToList();
            return Ok(new ListPage<PaperModelS2>(paperModels, works.Total));
        }

        /*[HttpGet]
        [Route("papers/s2/author")]
        [SwaggerOperation($"Gets papers from Semantic Scholar - search author")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The papers from S2", typeof(ListPage<PaperAuthorModelS2>))]
        public async Task<IActionResult> GetS2PapersAuthor([FromQuery] ExternalSearchModel model)
        {
            var authors = await _s2Facade.ListAuthorsAsync(model.Search, model.Page, model.PageSize);
            var authorModels = authors.Select(_mapper.Map<PaperAuthorModelS2>);
            return Ok(new ListPage<PaperAuthorModelS2>(authorModels, authors.Total));
        }*/

        [HttpGet]
        [Route("papers/oa/")]
        [SwaggerOperation($"Gets works from OpenAlex - search works")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The works from OpenAlex", typeof(ListPage<PaperModelOA>))]
        public async Task<IActionResult> GetOpenAlexWorks([FromQuery] ExternalSearchModel model)
        {
            var works = await _openAlexFacade.ListWorksAsync(model.Search, model.Page, model.PageSize);
            var paperModels = works.Items.Select(_mapper.Map<PaperModelOA>).ToList();
            return Ok(new ListPage<PaperModelOA>(paperModels, works.Total));
        }

        [HttpGet]
        [Route("papers/pm/")]
        [SwaggerOperation($"Gets articles from PubMed - search articles")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The articles from PubMed", typeof(ListPage<PaperModelPM>))]
        public async Task<IActionResult> GetPubMedArticles([FromQuery] ExternalSearchModel model)
        {
            var articles = await _pubMedFacade.ListArticlesAsync(model.Search, model.Page, model.PageSize);
            var paperModels = articles.Items.Select(_mapper.Map<PaperModelPM>).ToList();
            return Ok(new ListPage<PaperModelPM>(paperModels, articles.Total));
        }

        [HttpGet]
        [Route("papers/doi")]
        [SwaggerOperation($"Gets paper for DOI")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper found")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper", typeof(PaperModelOrcid))]
        public async Task<IActionResult> GetPaperForDOI([FromQuery] string id)
        {
            //Search OA first
            var oaWork = await _openAlexFacade.GetWorkFromDOI(id);

            if (oaWork != null)
            {
                var oaPaperModel = _mapper.Map<PaperModelOA>(oaWork);
                return Ok(oaPaperModel);
            }

            var orcidWork = await _orcidFacade.GetWorkFromDOI(id);

            if (orcidWork == null)
                return Ok();

            var orcidPaperModel = _mapper.Map<PaperModelOrcid>(orcidWork);
            return Ok(orcidPaperModel);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("{id}/notes")]
        //[SwaggerOperation($"Lists all notes for a given user")]
        //[SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        //[SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ContentEntityModel>))]
        //public async Task<IActionResult> GetNotes([SwaggerParameter("ID of the user")][FromRoute] string id, [FromQuery] SearchModel model)
        //{
        //    var notes = _contentService.ListNotesForUser(CurrentUserId, id, model.Search, model.Page, model.PageSize);
        //    var noteModels = notes.Select(_mapper.Map<ContentEntityModel>);
        //    return Ok(noteModels);
        //}

        [HttpGet]
        [Route("proposals")]
        [SwaggerOperation($"Lists all proposals for current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<ProposalModel>))]
        public async Task<IActionResult> GetProposals()
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            var proposals = _proposalService.ListForUser(CurrentUserId);
            var proposalModels = proposals.Select(_mapper.Map<ProposalModel>);
            return Ok(proposalModels);
        }

        [HttpGet]
        [Route("notifications/lastRead")]
        [SwaggerOperation($"Returns the date time and when the user last read their notifications")]
        public async Task<IActionResult> GetNotificationTimestamp()
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            return Ok(user.NotificationsReadUTC);
        }

        [HttpPost]
        [Route("notifications/lastRead")]
        [SwaggerOperation($"Records the date time and when the user last read their notifications")]
        public async Task<IActionResult> UpdateNotificationTimestamp()
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            user.NotificationsReadUTC = DateTime.UtcNow;
            await _userService.UpdateAsync(user);

            return Ok();
        }

        [Obsolete]
        [HttpGet]
        [Route("notifications")]
        [SwaggerOperation($"Lists all notifications for current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<NotificationModel>))]
        public async Task<IActionResult> GetNotifications([FromQuery] SearchModel model)
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            var notifications = _notificationService.ListSince(CurrentUserId, DateTime.UtcNow.AddYears(-1), model.Page, model.PageSize);
            var notificationModels = notifications.Items.Select(_mapper.Map<NotificationModel>);
            return Ok(notificationModels);
        }

        [HttpGet]
        [Route("current/notifications")]
        [SwaggerOperation($"Lists all notifications for current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(ListPage<NotificationModel>))]
        public async Task<IActionResult> GetNotificationsCurrent([FromQuery] SearchModel model)
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            var notifications = _notificationService.ListSince(CurrentUserId, DateTime.UtcNow.AddYears(-1), model.Page, model.PageSize);
            var notificationModels = notifications.Items.Select(_mapper.Map<NotificationModel>);
            return Ok(new ListPage<NotificationModel>(notificationModels, notifications.Total));
        }

        [HttpPost]
        [Route("notifications/{id}/actioned")]
        [SwaggerOperation($"Records the date time and when the user last read their notifications")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "The notification does not exist")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "The notification belongs to a different user")]
        public async Task<IActionResult> UpdateNotificationTimestamp([FromRoute] string id)
        {
            var notification = _notificationService.Get(id);
            if (notification == null)
                return NotFound();

            if (notification.UserId != CurrentUserId)
                return Forbid();

            if (notification.Actioned)
                return Ok();

            notification.Actioned = true;
            await _notificationService.UpdateAsync(notification);

            return Ok();
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/events")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        public async Task<IActionResult> GetEvents([FromRoute] string id, [FromQuery] SearchModel model, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] List<EventTag> tags)
        {
            var user = _userService.Get(id);
            if (user == null)
                return NotFound();

            var events = _eventService.ListForUser(id, CurrentUserId, tags, from, to, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var eventModels = events.Select(_mapper.Map<EventModel>).ToList();
            return Ok(eventModels);
        }

        [HttpGet]
        [Route("orcid/userdata")]
        [SwaggerOperation($"Gets data for the current user's ORCID")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No ORCID set on the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The data from ORCID", typeof(OrcidExperienceModel))]
        public async Task<IActionResult> GetUserOrcidData()
        {
            var user = _userService.Get(CurrentUserId);
            if (user == null)
                return NotFound();

            if (string.IsNullOrEmpty(user.Auth?.OrcidAccessToken))
            {
                user.Auth = new UserExternalAuth
                {
                    OrcidAccessToken = null
                };

                await _userService.UpdateAsync(user);
            }

            if (string.IsNullOrEmpty(user?.OrcidId) || string.IsNullOrEmpty(user.Auth?.OrcidAccessToken))
                return NotFound();

            var educationsTask = _orcidFacade.GetEducationsAsync(user.OrcidId, user.Auth.OrcidAccessToken);
            var employmentsTask = _orcidFacade.GetEmploymentsAsync(user.OrcidId, user.Auth.OrcidAccessToken);

            await Task.WhenAll(educationsTask, employmentsTask);

            var data = new OrcidExperienceModel
            {
                EducationItems = educationsTask.Result,
                EmploymentItems = employmentsTask.Result
            };

            return Ok(data);
        }

        [HttpGet]
        [Route("github/repos")]
        [SwaggerOperation($"Gets GitHub repositories for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The repository data", typeof(List<RepositoryModel>))]
        public async Task<IActionResult> GetGithubRepos([FromQuery] string accessToken)
        {
            var repos = await _githubFacade.GetReposAsync(accessToken);
            var repoModels = repos.Select(r => _mapper.Map<RepositoryModel>(r));
            return Ok(repoModels);
        }

        [HttpGet]
        [Route("huggingface/repos")]
        [SwaggerOperation($"Gets Hugging Face repositories for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The repository data", typeof(List<RepositoryModel>))]
        public async Task<IActionResult> GetHuggingFaceRepos([FromQuery] string accessToken)
        {
            var repos = await _huggingFaceFacade.GetReposAsync(accessToken);
            var repoModels = repos.Select(r => _mapper.Map<RepositoryModel>(r));
            return Ok(repoModels);
        }

        [HttpGet]
        [Route("linkedin/profile")]
        [SwaggerOperation($"Gets LinkedIn data for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The LinkedIn data", typeof(ProfileModel))]
        public async Task<IActionResult> GetLinkedinData([FromQuery] string linkedInUrl)
        {
            var data = await _lixFacade.GetProfileAsync(linkedInUrl);
            var dataModel = _mapper.Map<ProfileModel>(data);
            return Ok(dataModel);
        }

        [HttpPost]
        [Route("push/token")]
        [SwaggerOperation($"Upserts a push notification token for the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"Push notification token stored")]
        public async Task<IActionResult> UpsertPushNotificationToken([FromBody] string token)
        {
            await _userService.UpsertPushNotificationTokenAsync(token, CurrentUserId);
            return Ok();
        }

        [HttpGet]
        [Route("autocomplete")]
        [SwaggerOperation("Autocomplete search for all users")]
        [SwaggerResponse((int)HttpStatusCode.OK, "A list of user models", typeof(List<UserMiniModel>))]
        public async Task<IActionResult> AutocompleteUsers([FromQuery] SearchModel model)
        {
            var users = _userService.Autocomplete(model.Search, model.Page, model.PageSize);
            var userModels = users.Select(_mapper.Map<UserMiniModel>);
            return Ok(userModels);
        }
    }
}