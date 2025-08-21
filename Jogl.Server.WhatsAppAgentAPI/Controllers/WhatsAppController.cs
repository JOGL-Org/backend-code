using Jogl.Server.Conversation.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/")]
public class WhatsAppController : ControllerBase
{
    private readonly IInterfaceMessageRepository _interfaceMessageRepository;
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IInterfaceMessageRepository interfaceMessageRepository, IServiceBusProxy serviceBusProxy, ILogger<WhatsAppController> logger)
    {
        _interfaceMessageRepository = interfaceMessageRepository;
        _serviceBusProxy = serviceBusProxy;
        _logger = logger;
    }

    [HttpPost("webhook")]
    // [ValidateRequest]
    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioMessage payload)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);
        if (!string.IsNullOrEmpty(payload.OriginalRepliedMessageSid))
        {
            var originalMessage = _interfaceMessageRepository.Get(im => im.MessageId == payload.OriginalRepliedMessageSid);
            if (originalMessage == null)
            {
                _logger.LogError("No reference message found in data for message id {originalMessageId}", payload.OriginalRepliedMessageSid);
                return Ok();
            }

            await SendReply(payload, originalMessage.ConversationId);
            return Ok();
        }

        await SendNewConversation(payload);
        return Ok();
    }

    private async Task SendNewConversation(TwilioMessage payload)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);

        //loads latest message 
        await _serviceBusProxy.SendAsync(new Message
        {
            ConversationSystem = Const.TYPE_WHATSAPP,
            WorkspaceId = from,
            ChannelId = from,
            ConversationId = payload.MessageSid,
            Text = payload.Body,
            UserId = from,
        }, "conversation-created");
    }

    private async Task SendReply(TwilioMessage payload, string conversationId)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);

        //loads latest message 
        await _serviceBusProxy.SendAsync(new Message
        {
            ConversationSystem = Const.TYPE_WHATSAPP,
            WorkspaceId = from,
            ChannelId = from,
            ConversationId = conversationId,
            Text = payload.Body,
            UserId = from,
            MessageId = payload.MessageSid
        }, "conversation-reply-created");
    }
}