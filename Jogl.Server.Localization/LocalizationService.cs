using System.Reflection;
using System.Text.Json;

namespace Jogl.Server.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public string GetString(object key, string language ="en", params object[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"locale\\{language}.json");
            using var document = JsonDocument.Parse(stream);
            {
                var keyString = key.ToString();
                var segments = keyString.Split('.');
                var current = document.RootElement;

                foreach (var segment in segments)
                {
                    if (current.TryGetProperty(segment, out var next))
                        current = next;
                    else
                        return keyString;
                }

                var str= current.GetString();
                return string.Format(str, args);
            }
        }
    }
}
