using BaseService.Common.Settings;
using BaseService.Common.Utils.Const;
using BaseService.Infrastructure.Contexts;
using ClientService.Infrastructure.Contexts;
using Marten;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ClientService.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddClientServiceDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Load environment variables
        EnvLoader.Load();

        // EF Core DbContext
        var connectionString = Environment.GetEnvironmentVariable("CLIENT_SERVICE_DB")
                               ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<FormsDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseOpenIddict<Guid>();
        });

        // Register FormsDbContext as AppDbContext (required by BaseService repositories/UoW)
        services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<FormsDbContext>());

        // Marten (required by BaseService UnitOfWork constructor signature)
        services.AddMarten(connectionString!).UseLightweightSessions();

        // Redis (required by BaseService UnitOfWork constructor signature)
        var redisConnection = Environment.GetEnvironmentVariable(ConstEnv.RedisCacheConnection) ?? "localhost:6379";
        var redis = ConnectionMultiplexer.Connect(redisConnection);
        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddScoped<IDatabase>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        return services;
    }

    public static WebApplication UseClientServiceDatabaseMigrations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FormsDbContext>();
            dbContext.Database.Migrate();
        }

        return app;
    }
}
