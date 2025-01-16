using System.ComponentModel.Design;
using System.Reflection;
using System.Text.Json;

namespace Jogl.Server.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public string GetString(object key, string language = "en", params object[] args)
        {
            if (string.IsNullOrEmpty(language))
                language = "en";

            var assembly = Assembly.GetExecutingAssembly();
            var x = this.GetType().Assembly.GetManifestResourceNames();
            using var stream = assembly.GetManifestResourceStream($"Jogl.Server.Localization.locale.{language}.json");
            if (stream == null)
                return string.Empty;

            using var document = JsonDocument.Parse(stream);
            {
                var keyString = key.ToString();
                var segments = keyString.Split('.');
                var current = document.RootElement;

                foreach (var segment in segments)
                {
                    if (current.TryGetProperty(segment, out var next))
                        current = next;
                    else if (current.TryGetProperty(segment.ToLower(), out var nextLowercase))
                        current = nextLowercase;
                    else return keyString;
                }

                var str = current.GetString();
                return string.Format(str, args);
            }
        }
    }
}
