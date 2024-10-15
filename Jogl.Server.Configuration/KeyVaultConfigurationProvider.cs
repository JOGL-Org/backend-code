using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Configuration
{
    public sealed class KeyVaultConfigurationProvider : ConfigurationProvider
    {
        private readonly string _url;
        private readonly IConfiguration _configuration;
        public KeyVaultConfigurationProvider(string url, IConfiguration configuration)
        {
            _url = url;
            _configuration = configuration;
        }

        const string SECRET = "[SECRET]";

        public override void Load()
        {
            var client = new SecretClient(new Uri(_url), new DefaultAzureCredential());
            foreach (var val in _configuration.AsEnumerable())
            {
                if (val.Value == SECRET)
                {
                    var res = client.GetSecret(val.Key.Replace(":", "-"));
                    Data[val.Key] = res.Value.Value;
                }
            }
        }
    }
}
