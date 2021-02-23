using AuthNAuthZ.EncryptionProvider;
using AuthNAuthZ.TokenProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthNAuthZ.Extensions
{
    public static class AuthNAuthZConfigurationExtensions
    {
        public static void ConfigureTokenProvider<TTokenProvider>(
            this IServiceCollection services, 
            Func<ITokenProviderServiceSettings, IEncryptionProvider, TTokenProvider> constructor, 
            Action<TokenProviderServiceSettings> tpsHandler = null)
            where TTokenProvider : ITokenHandler
        {
            services.AddSingleton<ITokenHandler>(sp =>
            {
                var tokenProviderSettingsSection = sp.GetRequiredService<IConfiguration>().GetSection("TokenProviderSettings");
                var encryptionProviderSettingsSection = sp.GetRequiredService<IConfiguration>().GetSection("EncryptionProviderSettings");
                TokenProviderServiceSettings tps = tokenProviderSettingsSection != null ? new TokenProviderServiceSettings
                {
                    AudienceUrl = tokenProviderSettingsSection.GetValue<string>("AudienceUrl"),
                    ConfirmationMethod = tokenProviderSettingsSection.GetValue<string>("ConfirmationMethod"),
                    Issuer = tokenProviderSettingsSection.GetValue<string>("Issuer"),
                    Namespace = tokenProviderSettingsSection.GetValue<string>("Namespace"),
                    SubjectName = tokenProviderSettingsSection.GetValue<string>("SubjectName"),
                    ValidFor = tokenProviderSettingsSection.GetValue<int>("ValidFor"),
                } : new TokenProviderServiceSettings();
                if(tpsHandler != null)
                {
                    tpsHandler(tps);
                }
                IEncryptionProvider encryptionProvider = encryptionProviderSettingsSection != null ? 
                new EncryptionProvider.EncryptionProvider(encryptionProviderSettingsSection.GetValue<string>("Key")) : 
                new EncryptionProvider.EncryptionProvider($"{tps.AudienceUrl}{tps.ConfirmationMethod}{tps.Issuer}{tps.Namespace}{tps.SubjectName}{tps.ValidFor}");

                return constructor(tps, encryptionProvider);
            }
            );
        }
    }
}
