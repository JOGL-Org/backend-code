using AutoMapper;
using Jogl.Server.API.Model;
using Jogl.Server.Business;
using Jogl.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Jogl.Server.API.Services;

namespace Jogl.Server.API.Controllers
{
    [ApiController]
    [Route("public")]
    public class PublicController : BaseController
    {
        private readonly IUserService _userService;

        public PublicController(IUserService userService, IMapper mapper, ILogger<PublicController> logger, IEntityService entityService, IContextService contextService) : base(entityService, contextService, mapper, logger)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("join")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User successful added to the waitlist")]
        public async Task<IActionResult> Login([FromBody] WaitlistRecordModel model)
        {
            var data = _mapper.Map<WaitlistRecord>(model);
            await _userService.CreateWaitlistRecordAsync(data);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("contact")]
        [SwaggerResponse((int)HttpStatusCode.OK, "User contact successful")]
        public async Task<IActionResult> Login([FromBody] UserContactModel model)
        {
            var data = _mapper.Map<UserContact>(model);
            await _userService.SendContactEmailAsync(data);
            return Ok();
        }

    }
}