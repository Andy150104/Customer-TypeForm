using ClientService.API.HostedServices;
using ClientService.Infrastructure.Contexts;
using OpenIddict.Abstractions;

namespace ClientService.API.Extensions;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddClientServiceOpenIddict(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<FormsDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddServer(options =>
            {
                // Get issuer URL from environment variable to ensure tokens are issued with correct issuer
                var authServiceUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL");
                if (!string.IsNullOrWhiteSpace(authServiceUrl))
                {
                    var issuer = authServiceUrl.TrimEnd('/');
                    options.SetIssuer(new Uri(issuer));
                }
                
                options.SetTokenEndpointUris("/connect/token");
                options.SetIntrospectionEndpointUris("/connect/introspect");
                options.SetRevocationEndpointUris("/connect/revoke");

                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.AllowCustomFlow("google");
                
                // Make access_token an opaque reference token (same style as refresh_token).
                options.UseReferenceAccessTokens();
                options.UseReferenceRefreshTokens();
                
                // Disable encryption for reference tokens (they're stored in DB, not in token itself)
                options.DisableAccessTokenEncryption();

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess);

                // Use development certificates (persistent across restarts)
                // For reference tokens, we still need encryption key for internal operations,
                // but access tokens themselves are not encrypted
                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();
                
                // Set token lifetimes
                options.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                options.SetRefreshTokenLifetime(TimeSpan.FromHours(2));

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
            });

        // Seed OpenIddict applications (avoids invalid_client when DB is empty)
        services.AddHostedService<OpenIddictSeederHostedService>();

        return services;
    }
}

