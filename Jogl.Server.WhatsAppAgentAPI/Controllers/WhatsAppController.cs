using Jogl.Server.Conversation.Data;
using Jogl.Server.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;

[ApiController]
[Route("/")]
public class WhatsAppController : ControllerBase
{
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IServiceBusProxy serviceBusProxy, ILogger<WhatsAppController> logger)
    {
        _serviceBusProxy = serviceBusProxy;
        _logger = logger;
    }

    [HttpPost("webhook")]
    [ValidateRequest]
    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioMessage payload)
    {
        await _serviceBusProxy.SendAsync(new ConversationCreated
        {
            ConversationSystem = Const.TYPE_WHATSAPP,
            WorkspaceId = payload.From.Replace("whatsapp:", string.Empty),
            ChannelId = payload.From.Replace("whatsapp:", string.Empty),
            ConversationId = payload.MessageSid,
            Text = payload.Body,
            UserId = payload.From
        }, "conversation-created");

        return Ok();
    }
}