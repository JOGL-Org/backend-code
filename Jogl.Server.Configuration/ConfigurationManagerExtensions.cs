using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Configuration
{
    public static class ConfigurationManagerExtensions
    {
        public static ConfigurationManager AddKeyVault(this ConfigurationManager manager)
        {
            var keyVaultUrl = manager["Azure:KeyVault:URL"];
            if (string.IsNullOrEmpty(keyVaultUrl))
                throw new Exception("Missing configuration Azure:KeyVault:URL for key vault auto-config");

            IConfigurationBuilder configBuilder = manager;
            configBuilder.Add(new KeyVaultConfigurationSource(keyVaultUrl, manager));

            return manager;
        }
    }
}