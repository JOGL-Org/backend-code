using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Configuration
{
    public sealed class KeyVaultConfigurationSource : IConfigurationSource
    {
        private readonly string _url;
        private readonly IConfiguration _configuration;
        public KeyVaultConfigurationSource(string url, IConfiguration configuration)
        {
            _url = url;
            _configuration = configuration;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new KeyVaultConfigurationProvider(_url, _configuration);
    }
}
