using Jogl.Server.Conversation.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;

[ApiController]
[Route("/")]
public class WhatsAppController : ControllerBase
{
    private readonly IInterfaceUserRepository _interfaceUserRepository;
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IInterfaceUserRepository interfaceUserRepository, IServiceBusProxy serviceBusProxy, ILogger<WhatsAppController> logger)
    {
        _interfaceUserRepository = interfaceUserRepository;
        _serviceBusProxy = serviceBusProxy;
        _logger = logger;
    }

    [HttpPost("webhook")]
    [ValidateRequest]
    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioMessage payload)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);
        var user = _interfaceUserRepository.Get(iu => iu.ExternalId == from);
        if (user == null)
        {
            user = new Jogl.Server.Data.InterfaceUser
            {
                ExternalId = from
            };

            await _interfaceUserRepository.CreateAsync(user);
        }

        if (user.Status == Jogl.Server.Data.InterfaceUserStatus.InThread)
            await _serviceBusProxy.SendAsync(new ConversationReplyCreated
            {
                ConversationSystem = Const.TYPE_WHATSAPP,
                WorkspaceId = from,
                ChannelId = from,
                ConversationId = payload.MessageSid,
                Text = payload.Body,
                UserId = payload.From,
                MessageId = payload.MessageSid
            }, "conversation-reply-created");
        else
            await _serviceBusProxy.SendAsync(new ConversationCreated
            {
                ConversationSystem = Const.TYPE_WHATSAPP,
                WorkspaceId = from,
                ChannelId = from,
                ConversationId = payload.MessageSid,
                Text = payload.Body,
                UserId = payload.From
            }, "conversation-created");

        return Ok();
    }
}