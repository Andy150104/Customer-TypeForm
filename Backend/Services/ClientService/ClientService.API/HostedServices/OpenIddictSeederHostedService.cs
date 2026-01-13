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
        if (existing != null)
        {
            // Update existing application to include google grant type permission
            var existingDescriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(existingDescriptor, existing, cancellationToken);
            var googlePermission = OpenIddictConstants.Permissions.Prefixes.GrantType + "google";
            
            if (!existingDescriptor.Permissions.Contains(googlePermission))
            {
                existingDescriptor.Permissions.Add(googlePermission);
                await manager.UpdateAsync(existing, existingDescriptor, cancellationToken);
            }
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = clientId,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        };

        // Minimal permissions for password + refresh token + google flow.
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        // Add permission for custom "google" grant type
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + "google");

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Roles);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess);

        await manager.CreateAsync(descriptor, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

