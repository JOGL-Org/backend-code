namespace Jogl.Server.PushNotifications
{
    public interface IPushNotificationService
    {
        Task PushAsync(string token, string title, string body, string link);
        Task PushAsync(List<string> token, string title, string body, string link);
    }
}