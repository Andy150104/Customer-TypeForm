using BaseService.Common.Settings;
using BaseService.Common.Utils.Const;
using OpenIddict.Validation.AspNetCore;

namespace ClientService.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Configure OpenIddict Authentication for ClientService
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddClientServiceAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Load environment variables from .env file
        EnvLoader.Load();

        // Configure OpenIddict Validation
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                // Use same database context as server to validate tokens
                options.UseEntityFrameworkCore()
                    .UseDbContext<ClientService.Infrastructure.Contexts.FormsDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddValidation(options =>
            {
                // Get issuer URL from environment variable
                var authServiceUrl = Environment.GetEnvironmentVariable(ConstEnv.AuthServiceUrl);
                if (string.IsNullOrWhiteSpace(authServiceUrl))
                {
                    throw new InvalidOperationException($"Environment variable '{ConstEnv.AuthServiceUrl}' is not set.");
                }
                
                // Remove trailing slash for issuer
                var issuer = authServiceUrl.TrimEnd('/');
                
                // Get audience from environment variables
                var audience = Environment.GetEnvironmentVariable(ConstEnv.JwtAudience);
                if (string.IsNullOrWhiteSpace(audience))
                {
                    throw new InvalidOperationException($"Environment variable '{ConstEnv.JwtAudience}' is not set.");
                }

                // Get client credentials from environment variables (must match ClientService's OpenIddict application)
                var clientId = Environment.GetEnvironmentVariable(ConstEnv.ClientId);
                var clientSecret = Environment.GetEnvironmentVariable(ConstEnv.ClientSecret);
                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new InvalidOperationException($"Environment variables '{ConstEnv.ClientId}' or '{ConstEnv.ClientSecret}' are not set.");
                }

                // Use introspection endpoint to validate reference tokens
                // Since tokens are issued by ClientService itself, introspection will call /connect/introspect
                // OpenIddict will automatically construct introspection endpoint as {issuer}/connect/introspect
                options.SetIssuer(issuer);
                options.UseIntrospection()
                    .AddAudiences(audience)
                    .SetClientId(clientId)
                    .SetClientSecret(clientSecret);

                // Integrate with ASP.NET Core
                // Configure HttpClient to bypass SSL certificate validation for development
                options.UseSystemNetHttp(httpClient =>
                {
                    httpClient.ConfigureHttpClientHandler(handler =>
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            // In development, allow self-signed certificates
                            return true;
                        };
                    });
                });
                options.UseAspNetCore();
            });

        return services;
    }
}
