//using Jogl.Server.Conversation.Data;
//using Jogl.Server.Data;
//using Jogl.Server.DB;
//using Jogl.Server.ServiceBus;
//using Microsoft.AspNetCore.Mvc;
//using Twilio.AspNet.Core;

//[ApiController]
//[Route("/")]
//public class WhatsAppController : ControllerBase
//{
//    private readonly IInterfaceUserRepository _interfaceUserRepository;
//    private readonly IInterfaceMessageRepository _interfaceMessageRepository;
//    private readonly IServiceBusProxy _serviceBusProxy;
//    private readonly ILogger<WhatsAppController> _logger;

//    public WhatsAppController(IInterfaceUserRepository interfaceUserRepository, IInterfaceMessageRepository interfaceMessageRepository, IServiceBusProxy serviceBusProxy, ILogger<WhatsAppController> logger)
//    {
//        _interfaceUserRepository = interfaceUserRepository;
//        _interfaceMessageRepository = interfaceMessageRepository;
//        _serviceBusProxy = serviceBusProxy;
//        _logger = logger;
//    }

//    [HttpPost("webhook")]
//    [ValidateRequest]
//    public async Task<IActionResult> ReceiveMessage([FromForm] TwilioMessage payload)
//    {
//        if (!string.IsNullOrEmpty(payload.Body))
//            await ProcessTextAsync(payload);
//        else
//            await ProcessButtonClickAsync(payload);
//        return Ok();
//    }

//    private async Task ProcessTextAsync(TwilioMessage payload)
//    {
//        var from = payload.From.Replace("whatsapp:", string.Empty);
//        var user = await EnsureInterfaceUserAsync(from);

//        if (user.Status == InterfaceUserStatus.InThread)
//        {
//            //loads latest message 
//            var rootMessage = _interfaceMessageRepository
//                .Query(im => im.UserId == from && im.Context != null)
//                .Sort(Jogl.Server.Data.Util.SortKey.Date, false)
//                .Page(1, 1)
//                .ToList()
//                .Single();

//            await _serviceBusProxy.SendAsync(new ConversationReplyCreated
//            {
//                ConversationSystem = Const.TYPE_WHATSAPP,
//                WorkspaceId = from,
//                ChannelId = from,
//                ConversationId = rootMessage.ConversationId,
//                Text = payload.Body,
//                UserId = from,
//                MessageId = payload.MessageSid
//            }, "conversation-reply-created");

//            user.Status = InterfaceUserStatus.None;
//            await _interfaceUserRepository.UpdateAsync(user);
//        }
//        else
//            await _serviceBusProxy.SendAsync(new ConversationCreated
//            {
//                ConversationSystem = Const.TYPE_WHATSAPP,
//                WorkspaceId = from,
//                ChannelId = from,
//                ConversationId = payload.MessageSid,
//                Text = payload.Body,
//                UserId = from,
//            }, "conversation-created");
//    }

//    private async Task ProcessButtonClickAsync(TwilioMessage payload)
//    {
//        var from = payload.From.Replace("whatsapp:", string.Empty);
//        var user = await EnsureInterfaceUserAsync(from);

//        if (user.Status != InterfaceUserStatus.None)
//            return;

//        user.Status = InterfaceUserStatus.InThread;
//        await _interfaceUserRepository.UpdateAsync(user);
//    }

//    private async Task<InterfaceUser> EnsureInterfaceUserAsync(string from)
//    {
//        var user = _interfaceUserRepository.Get(iu => iu.ExternalId == from);
//        if (user == null)
//        {
//            user = new InterfaceUser
//            {
//                ExternalId = from
//            };

//            await _interfaceUserRepository.CreateAsync(user);
//        }

//        return user;
//    }
//}