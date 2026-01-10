namespace ClientService.API.Extensions;

public static class CorsExtensions
{
    private const string DefaultPolicy = "ClientServiceCors";

    public static IServiceCollection AddClientServiceCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultPolicy, policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static WebApplication UseClientServiceCors(this WebApplication app)
    {
        app.UseCors(DefaultPolicy);
        return app;
    }
}

