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
                options.SetTokenEndpointUris("/connect/token");
                options.SetIntrospectionEndpointUris("/connect/introspect");
                options.SetRevocationEndpointUris("/connect/revoke");

                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                // Make access_token an opaque reference token (same style as refresh_token).
                options.UseReferenceAccessTokens();
                options.UseReferenceRefreshTokens();

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess);

                // OpenIddict requires at least one encryption key (even if access token encryption is disabled).
                // For development, use an ephemeral key.
                options.AddEphemeralEncryptionKey();
                options.AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
            });

        // Seed OpenIddict applications (avoids invalid_client when DB is empty)
        services.AddHostedService<OpenIddictSeederHostedService>();

        return services;
    }
}

