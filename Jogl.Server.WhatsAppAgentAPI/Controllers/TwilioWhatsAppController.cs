using Jogl.Server.AI;
using Jogl.Server.Conversation.Data;
using Jogl.Server.Data;
using Jogl.Server.DB;
using Jogl.Server.ServiceBus;
using Jogl.Server.WhatsApp;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/")]
public class TwilioWhatsAppController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IInterfaceMessageRepository _interfaceMessageRepository;
    private readonly ISystemValueRepository _systemValueRepository;
    private readonly IWhatsAppService _whatsappService;
    private readonly IServiceBusProxy _serviceBusProxy;
    private readonly ILogger<TwilioWhatsAppController> _logger;

    public TwilioWhatsAppController(IAIService aiService, IInterfaceMessageRepository interfaceMessageRepository, ISystemValueRepository systemValueRepository, IWhatsAppService whatsAppService, IServiceBusProxy serviceBusProxy, ILogger<TwilioWhatsAppController> logger)
    {
        _aiService = aiService;
        _interfaceMessageRepository = interfaceMessageRepository;
        _systemValueRepository = systemValueRepository;
        _whatsappService = whatsAppService;
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
            .Query(im => im.ChannelId == from && !string.IsNullOrEmpty(im.Tag))
            .Sort(Jogl.Server.Data.Util.SortKey.CreatedDate, false)
            .Page(1, 1)
            .ToList()
            .SingleOrDefault();

        if (rootMessage == null)
        {
            await SendMessageAsync(payload);
            return Ok();
        }

        var msgs = await _whatsappService.GetConversationAsync(from, rootMessage.MessageId);
        var prompt = _systemValueRepository.Get(sv => sv.Key == "ROUTER_PROMPT");
        if (prompt == null)
        {
            _logger.LogError("ROUTER_PROMPT system value missing");
            return UnprocessableEntity();
        }

        _logger.LogInformation("{payload}", payload.Body);
        var type = await _aiService.GetResponseAsync(prompt.Value, [.. msgs.Select(msg => new InputItem { FromUser = msg.FromUser, Text = msg.Text }), new InputItem { FromUser = true, Text = payload.Body }], 0);
        _logger.LogInformation("Identified as {type}", type);

        await SendMessageAsync(payload, rootMessage, type);
        return Ok();
    }

    private async Task SendMessageAsync(TwilioMessage payload, InterfaceMessage? rootMessage = default, string? type = default)
    {
        var from = payload.From.Replace("whatsapp:", string.Empty);
        await _serviceBusProxy.SendAsync(new Message
        {
            ConversationSystem = Const.TYPE_WHATSAPP,
            WorkspaceId = from,
            ChannelId = from,
            ConversationId = type == "deepdive" ? rootMessage.ConversationId : payload.MessageSid,
            Text = payload.Body,
            UserId = from,
            Type = type,
            MessageId = payload.MessageSid,
        }, "interface-message-received");
    }
}