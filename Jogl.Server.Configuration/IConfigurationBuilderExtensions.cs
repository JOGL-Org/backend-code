using Microsoft.Extensions.Configuration;

namespace Jogl.Server.Configuration
{
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddKeyVault(this IConfigurationBuilder builder)
        {
            var config = builder.Build();
            var keyVaultUrl = config["Azure:KeyVault:URL"];
            if (string.IsNullOrEmpty(keyVaultUrl))
                throw new Exception("Missing configuration Azure:KeyVault:URL for key vault auto-config");

            builder.Add(new KeyVaultConfigurationSource(keyVaultUrl, config));
            return builder;
        }
    }
}