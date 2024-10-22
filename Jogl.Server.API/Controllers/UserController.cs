using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.API.Services;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Jogl.Server.Data.Enum;
using Jogl.Server.Data.Util;
using Jogl.Server.Email;
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
        private readonly ISemanticScholarFacade _s2Facade;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IUserVerificationService userVerificationService, IProposalService proposalService, IWorkspaceService workspaceService, INodeService nodeService, IOrganizationService organizationService, INeedService needService, IInvitationService invitationService, IContentService contentService, ICommunityEntityService communityEntityService, ITagService tagService, IEventService eventService, IPaperService paperService, IDocumentService documentService, INotificationService notificationService, IEmailService emailService, IOpenAlexFacade openAlexFacade, IOrcidFacade orcidFacade, ISemanticScholarFacade s2Facade, IPubMedFacade pubMedFacade, IConfiguration configuration, IMapper mapper, ILogger<UserController> logger, IVerificationService verificationService, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _userService = userService;
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
            _s2Facade = s2Facade;
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
            var existingUserEmail = _userService.GetForEmail(model.Email);
            if (existingUserEmail != null)
                return Conflict(new ErrorModel("email"));

            if (!string.IsNullOrEmpty(model.Username))
            {
                //check for duplicate usernames
                var existingUserUsername = _userService.GetForUsername(model.Username);
                if (existingUserUsername != null)
                    return Conflict(new ErrorModel("username"));
            }
            else
            {
                //autogenerate username
                model.Username = _userService.GetUniqueUsername(model.FirstName, model.LastName);
            }

            //create user
            var user = _mapper.Map<User>(model);
            await InitCreationAsync(user);
            var userId = await _userService.CreateAsync(user, model.Password);

            //if no verification code present, trigger verification
            if (string.IsNullOrEmpty(model.VerificationCode))
            {
                await _userVerificationService.CreateAsync(user, VerificationAction.Verify, model.RedirectURL, true);
                return Ok(userId);
            }

            //if no verification code present, process
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
        [Route("check")]
        [SwaggerOperation($"Verifies whether an email address is eligible to join JOGL")]
        [SwaggerResponse((int)HttpStatusCode.OK, "The email address can be used to sign up with a new user")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "A user with this email already exists")]
        public async Task<IActionResult> CheckEmail([FromBody] EmailModel model)
        {
            var existingUserEmail = _userService.GetForEmail(model.Email);
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
        public async Task<IActionResult> GetSpaces([FromRoute] string id, [FromQuery] Permission? permission, [FromQuery] SearchModel model)
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

        [AllowAnonymous]
        [HttpGet]
        [Route("exists/{username}")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "Username is already taken")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Username is available")]
        public async Task<IActionResult> Exists([FromRoute] string username)
        {
            var user = _userService.GetForUsername(username);
            if (user == null)
                return Ok();

            return Conflict();
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

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/activity")]
        public async Task<IActionResult> ListActivityContentEntities([FromRoute] string id, [FromQuery] SearchModel model, [FromQuery] bool loadDetails)
        {
            var contentObjects = _contentService.ListActivity(id, model.Search, model.Page, model.PageSize, CurrentUserId, loadDetails);
            var contentObjectModels = contentObjects.Select(_mapper.Map<ActivityRecordModel>);
            return Ok(contentObjectModels);
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
        public async Task<IActionResult> GetInterests([FromQuery] SearchModel model)
        {
            var skills = _tagService.GetTags(model.Search, model.Page, model.PageSize);
            var skillModels = skills.Select(_mapper.Map<TextValueModel>);
            return Ok(skillModels);
        }

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
            await InitCreationAsync(paper);
            var paperId = await _paperService.CreateAsync(CurrentUserId, paper);

            return Ok(paperId);
        }

        [HttpPost]
        [Route("papers/{paperId}")]
        [SwaggerOperation($"Associates a paper to the current user")]
        public async Task<IActionResult> AssociatePaper([FromRoute] string paperId)
        {
            await _paperService.AssociateAsync(CurrentUserId, paperId);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers/authored")]
        [SwaggerOperation($"Lists all papers authored by the specified user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<PaperModel>))]
        public async Task<IActionResult> GetAuthorPapers([SwaggerParameter("ID of the user")][FromRoute] string id, [SwaggerParameter("The paper type")][FromQuery] PaperType? type, [FromQuery] SearchModel model)
        {
            var papers = _paperService.ListForAuthor(CurrentUserId, id, type, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Select(_mapper.Map<PaperModel>);
            return Ok(paperModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}/papers")]
        [SwaggerOperation($"Lists all papers associated to the specified user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No user was found for that id")]
        [SwaggerResponse((int)HttpStatusCode.OK, "", typeof(List<PaperModel>))]
        public async Task<IActionResult> GetPapers([SwaggerParameter("ID of the user")][FromRoute] string id, [SwaggerParameter("The paper type")][FromQuery] PaperType? type, [FromQuery] List<PaperTag> tags, [FromQuery] SearchModel model)
        {
            var papers = _paperService.ListForEntity(CurrentUserId, id, type, tags, model.Search, model.Page, model.PageSize, model.SortKey, model.SortAscending);
            var paperModels = papers.Select(_mapper.Map<PaperModel>);
            return Ok(paperModels);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("papers/{paperId}")]
        [SwaggerOperation($"Returns a single paper")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for the paper id")]
        public async Task<IActionResult> GetPaper([FromRoute] string paperId)
        {
            var paper = _paperService.Get(paperId, CurrentUserId);
            if (paper == null)
                return NotFound();

            var paperModel = _mapper.Map<PaperModel>(paper);
            return Ok(paperModel);
        }

        [HttpDelete]
        [Route("papers/{paperId}")]
        [SwaggerOperation($"Disassociates the specified paper from the current user")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "No paper was found for the paper id or the paper isn't associated to the current user")]
        [SwaggerResponse((int)HttpStatusCode.OK, $"The paper was deleted")]
        public async Task<IActionResult> DeletePaper([FromRoute] string paperId)
        {
            var paper = _paperService.Get(paperId, CurrentUserId);
            if (paper == null)
                return NotFound();

            if (!paper.FeedIds.Contains(CurrentUserId))
                return NotFound();

            await _paperService.DisassociateAsync(CurrentUserId, paperId);
            return Ok();
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
            var existingPapers = _paperService.ListForExternalIds(paperModels.Select(p => p.ExternalId));

            foreach (var paper in paperModels)
            {
                paper.OnJogl = existingPapers.Any(p => p.ExternalId == paper.ExternalId);
            }

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
            EnrichExternalPaperModels(paperModels);

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
            EnrichExternalPaperModels(paperModels);
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
            EnrichExternalPaperModels(paperModels);
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
            var notificationModels = notifications.Select(_mapper.Map<NotificationModel>);
            return Ok(notificationModels);
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

        //[HttpPost]
        //[Route("testEmail")]
        //[SwaggerOperation($"Sends a test email to a specific address")]
        //[SwaggerResponse((int)HttpStatusCode.Forbidden, $"Bad user, bad")]
        //[SwaggerResponse((int)HttpStatusCode.OK, $"Email sent")]
        //public async Task<IActionResult> SendEmailAsync([FromBody] EmailMessageModel model)
        //{
        //    var user = _userService.Get(CurrentUserId);
        //    switch (user.Email)
        //    {
        //        case "filip@jogl.io":
        //        case "thomas@jogl.io":
        //        case "louis@jogl.io":
        //            await _emailService.SendEmailAsync(model.ToEmail, EmailTemplate.Test, new { body = model.Body, subject = model.Subject }, fromName: model.FromName);
        //            return Ok();
        //        default:
        //            return Forbid();
        //    }
        //}

        private void EnrichExternalPaperModels(IEnumerable<ExternalPaperModel> externalPaperModels)
        {
            var joglPapers = _paperService.ListForExternalIds(externalPaperModels.Select(p => p.ExternalId).ToList());
            foreach (var joglPaper in joglPapers)
            {
                var externalPaperModel = externalPaperModels.FirstOrDefault(pm => pm.ExternalId == joglPaper.ExternalId);
                if (externalPaperModel != null)
                    externalPaperModel.JoglId = joglPaper.Id.ToString();
            }
        }

        //[HttpGet]
        //[AllowAnonymous]
        //[Route("migrations/bots")]
        //public async Task<IActionResult> PurgeBotUsers()
        //{
        //    foreach (var id in System.IO.File.ReadAllLines("bots.txt"))
        //    {
        //        await _userService.DeleteAsync(id);
        //    }

        //    return Ok();
        //}
    }
}