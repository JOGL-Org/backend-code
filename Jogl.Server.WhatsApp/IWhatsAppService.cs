namespace Jogl.Server.WhatsApp
{
    public interface IWhatsAppService
    {
        Task<string> SendMessageAsync(string number, string message);
        Task SendMessageButtonAsync(string number);
    }
}
