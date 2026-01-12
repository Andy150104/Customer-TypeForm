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
            .AddValidation(options =>
            {
                // Tokens are issued by ClientService (via /connect/token endpoint)
                // So validation must validate from ClientService itself (self-introspection)
                // Get ClientService URL from configuration
                var urls = configuration.GetValue<string>("ASPNETCORE_URLS")?.Split(';') ?? Array.Empty<string>();
                var clientServiceUrl = urls.FirstOrDefault(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    ?? urls.FirstOrDefault()
                    ?? "https://localhost:7217";
                
                // Remove trailing slash for issuer
                // OpenIddict will automatically construct introspection endpoint as {issuer}/connect/introspect
                var issuer = clientServiceUrl.TrimEnd('/');
                options.SetIssuer(issuer);

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

                // Use introspection endpoint to validate reference tokens from ClientService itself (self-introspection)
                // OpenIddict automatically constructs the introspection endpoint URL as {issuer}/connect/introspect
                // This will call ClientService's /connect/introspect endpoint to validate tokens
                options.UseIntrospection()
                    .AddAudiences(audience)
                    .SetClientId(clientId)
                    .SetClientSecret(clientSecret);

                // Integrate with ASP.NET Core
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        return services;
    }
}
