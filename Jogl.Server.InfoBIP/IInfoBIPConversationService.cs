namespace Jogl.Server.InfoBIP
{
    public interface IInfoBIPConversationService
    {
        Task<string> SendWhatsappMessageAsync(string from, string to, string message);
    }
}
