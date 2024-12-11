namespace Jogl.Server.Localization
{
    public interface ILocalizationService
    {
        string GetString(object key, string language = "en", params object[] args);
    }
}
