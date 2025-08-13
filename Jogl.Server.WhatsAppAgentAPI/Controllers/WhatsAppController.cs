using Jogl.Server.AI;
using Jogl.Server.Conversation.Data;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;

[ApiController]
[Route("/")]
public class WhatsAppController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IInterfaceMessageRepository _interfaceMessageRepository;
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IAIService aiService, IInterfaceMessageRepository interfaceMessageRepository, IServiceBusProxy serviceBusProxy, ILogger<WhatsAppController> logger)
    {
        _aiService = aiService;
        _interfaceMessageRepository = interfaceMessageRepository;
        //   _whatsappService = whatsAppService;
        _serviceBusProxy = serviceBusProxy;
        _logger = logger;
    }

    [HttpPost("webhook")]
    // [ValidateRequest]
    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioMessage payload)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);

        //loads latest message
        var rootMessage = _interfaceMessageRepository
            .Query(im => im.UserId == from && im.Context != null)
            .Sort(Jogl.Server.Data.Util.SortKey.Date, false)
            .Page(1, 1)
            .ToList()
            .SingleOrDefault();

        if (rootMessage == null)
        {
            await SendNewConversation(payload);
            return Ok();
        }

        var isReply = await _aiService.GetResponseAsync<bool>("Return true or false, indicating whether or not the message is a followup to the previous conversation (true), or a new conversation (false)", [
            new InputItem { FromUser = true, Text = rootMessage.Text },
            new InputItem { FromUser = false, Text = rootMessage.Context },
            new InputItem { FromUser = true, Text = payload.Body }],
            0);

        _logger.LogInformation("{payload} identified as reply: {isReply}", payload.Body, isReply);

        if (isReply)
            await SendReply(payload, rootMessage);
        else
            await SendNewConversation(payload);

        return Ok();
    }

    private async Task SendNewConversation(TwilioMessage payload)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);

        //loads latest message 
        await _serviceBusProxy.SendAsync(new ConversationCreated
        {
            ConversationSystem = Const.TYPE_WHATSAPP,
            WorkspaceId = from,
            ChannelId = from,
            ConversationId = payload.MessageSid,
            Text = payload.Body,
            UserId = from,
        }, "conversation-created");
    }

    private async Task SendReply(TwilioMessage payload, InterfaceMessage message)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);

        //loads latest message 
        await _serviceBusProxy.SendAsync(new ConversationReplyCreated
        {
            ConversationSystem = Const.TYPE_WHATSAPP,
            WorkspaceId = from,
            ChannelId = from,
            ConversationId = message.ConversationId,
            Text = payload.Body,
            UserId = from,
            MessageId = payload.MessageSid
        }, "conversation-reply-created");
    }
}