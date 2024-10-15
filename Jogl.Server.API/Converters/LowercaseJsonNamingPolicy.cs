using System.Text.Json;

namespace Jogl.Server.API.Converters
{
    public class LowercaseJsonNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToString().ToLower();
        }
    }
}
