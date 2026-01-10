using BaseService.Common.Utils.Const;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace ClientService.API.HostedServices;

/// <summary>
/// Ensure required OpenIddict applications exist so /connect/token doesn't fail with invalid_client.
/// </summary>
public class OpenIddictSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public OpenIddictSeederHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Prefer env vars if present; fallback to the sample values used in Postman.
        var clientId = Environment.GetEnvironmentVariable(ConstEnv.ClientId) ?? "service_client";
        var clientSecret = Environment.GetEnvironmentVariable(ConstEnv.ClientSecret) ?? "EduSmartAI-Capstone-Project";

        var existing = await manager.FindByClientIdAsync(clientId, cancellationToken);
        if (existing != null) return;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = clientId,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        };

        // Minimal permissions for password + refresh token flow.
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess);

        await manager.CreateAsync(descriptor, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

